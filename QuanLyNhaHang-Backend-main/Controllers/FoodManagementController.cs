using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHangAPI.Data;
using QuanLyNhaHangAPI.Data.Entities;
using QuanLyNhaHangAPI.Models;
using QuanLyNhaHangAPI.Services;

namespace QuanLyNhaHangAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodManagementController : ControllerBase
    {
        private readonly QuanLyNhaHangDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public FoodManagementController(
            QuanLyNhaHangDbContext context,
            ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        // Upload ảnh món ăn lên Cloudinary, trả về URL để lưu vào trường HinhAnh
        [HttpPost("upload-image")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadImage(
            [FromForm] UploadImageRequest request)
        {
            var file = request.File;

            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Vui lòng chọn ảnh để tải lên"
                });
            }

            try
            {
                var (url, publicId) =
                    await _cloudinaryService.UploadImageAsync(file);

                return Ok(new
                {
                    url,
                    publicId
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Tải ảnh lên thất bại: {ex.Message}"
                });
            }
        }

        // Lấy danh sách món ăn
        [HttpGet]
        public async Task<IActionResult> GetFoods()
        {
            var foods = await _context.MonAn
                .Include(x => x.MaDanhMucNavigation)
                .Select(x => new
                {
                    x.MaMonAn,
                    x.TenMonAn,
                    x.Gia,
                    x.HinhAnh,
                    x.MoTa,
                    x.ConBan,
                    x.MaDanhMuc,
                    DanhMuc =
                        x.MaDanhMucNavigation.TenDanhMuc
                })
                .ToListAsync();

            return Ok(foods);
        }

        // Thêm món ăn
        [HttpPost]
        public async Task<IActionResult> CreateFood(
            CreateFoodRequest request)
        {
            var food = new MonAn
            {
                MaDanhMuc = request.MaDanhMuc,
                TenMonAn = request.TenMonAn,
                Gia = request.Gia,
                MoTa = request.MoTa,
                HinhAnh = request.HinhAnh,
                ConBan = request.ConBan
            };

            _context.MonAn.Add(food);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Thêm món ăn thành công"
            });
        }

        // Cập nhật món ăn
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFood(
            int id,
            UpdateFoodRequest request)
        {
            var food = await _context.MonAn
                .FindAsync(id);

            if (food == null)
            {
                return NotFound(
                    "Không tìm thấy món ăn");
            }

            // Nếu ảnh cũ là ảnh trên Cloudinary và đã được thay bằng ảnh mới khác,
            // xóa ảnh cũ trên Cloudinary để tránh rác dữ liệu
            var oldImage = food.HinhAnh;

            food.MaDanhMuc = request.MaDanhMuc;
            food.TenMonAn = request.TenMonAn;
            food.Gia = request.Gia;
            food.MoTa = request.MoTa;
            food.HinhAnh = request.HinhAnh;
            food.ConBan = request.ConBan;

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(oldImage)
                && oldImage != request.HinhAnh
                && oldImage.Contains("res.cloudinary.com"))
            {
                await TryDeleteCloudinaryImageAsync(oldImage);
            }

            return Ok(new
            {
                message = "Cập nhật thành công"
            });
        }

        // Xóa món ăn
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFood(
            int id)
        {
            var food = await _context.MonAn
                .FindAsync(id);

            if (food == null)
            {
                return NotFound(
                    "Không tìm thấy món ăn");
            }

            var imageUrl = food.HinhAnh;

            _context.MonAn.Remove(food);

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(imageUrl) && imageUrl.Contains("res.cloudinary.com"))
            {
                await TryDeleteCloudinaryImageAsync(imageUrl);
            }

            return Ok(new
            {
                message = "Xóa thành công"
            });
        }

        // Lấy public id từ URL Cloudinary và xóa ảnh tương ứng.
        // Lỗi (nếu có) sẽ được bỏ qua để không ảnh hưởng tới luồng nghiệp vụ chính.
        private async Task TryDeleteCloudinaryImageAsync(string imageUrl)
        {
            try
            {
                // Ví dụ URL: https://res.cloudinary.com/<cloud>/image/upload/v123456/quanlynhahang/monan/abc123.jpg
                // => publicId cần xóa là: quanlynhahang/monan/abc123
                var uri = new Uri(imageUrl);
                var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

                var uploadIndex = Array.IndexOf(segments, "upload");
                if (uploadIndex < 0 || uploadIndex + 1 >= segments.Length)
                {
                    return;
                }

                // Bỏ qua phần "upload" và phần version (vXXXXXXXXXX) nếu có
                var publicIdParts = segments.Skip(uploadIndex + 1)
                    .Where(s => !(s.StartsWith("v") && s.Length > 1 && s.Skip(1).All(char.IsDigit)))
                    .ToArray();

                if (publicIdParts.Length == 0)
                {
                    return;
                }

                // Bỏ phần đuôi file (.jpg, .png, ...) ở segment cuối cùng
                var lastSegment = publicIdParts[^1];
                var dotIndex = lastSegment.LastIndexOf('.');
                if (dotIndex > 0)
                {
                    publicIdParts[^1] = lastSegment[..dotIndex];
                }

                var publicId = string.Join("/", publicIdParts);

                await _cloudinaryService.DeleteImageAsync(publicId);
            }
            catch
            {
                // Không làm gián đoạn nghiệp vụ chính nếu xóa ảnh thất bại
            }
        }
    }
}
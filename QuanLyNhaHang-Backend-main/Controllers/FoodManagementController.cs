using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHangAPI.Data;
using QuanLyNhaHangAPI.Data.Entities;
using QuanLyNhaHangAPI.Models;

namespace QuanLyNhaHangAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodManagementController : ControllerBase
    {
        private readonly QuanLyNhaHangDbContext _context;

        public FoodManagementController(
            QuanLyNhaHangDbContext context)
        {
            _context = context;
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

            food.MaDanhMuc = request.MaDanhMuc;
            food.TenMonAn = request.TenMonAn;
            food.Gia = request.Gia;
            food.MoTa = request.MoTa;
            food.HinhAnh = request.HinhAnh;
            food.ConBan = request.ConBan;

            await _context.SaveChangesAsync();

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

            _context.MonAn.Remove(food);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Xóa thành công"
            });
        }
    }
}
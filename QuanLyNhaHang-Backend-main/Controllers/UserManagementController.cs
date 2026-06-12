using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHangAPI.Data;
using QuanLyNhaHangAPI.Data.Entities;
using QuanLyNhaHangAPI.Models;

namespace QuanLyNhaHangAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly QuanLyNhaHangDbContext _context;

        public UserManagementController(
            QuanLyNhaHangDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách nhân viên
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.NguoiDung
                .Include(x => x.MaVaiTroNavigation)
                .Select(x => new
                {
                    x.MaNguoiDung,
                    x.HoTen,
                    x.Email,
                    x.DienThoai,
                    VaiTro = x.MaVaiTroNavigation.TenVaiTro,
                    x.TrangThaiHoatDong
                })
                .ToListAsync();

            return Ok(users);
        }

        // Lấy thông tin 1 nhân viên
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(
            int id)
        {
            var user = await _context.NguoiDung
                .Where(x => x.MaNguoiDung == id)
                .Select(x => new
                {
                    hoTen = x.HoTen,
                    email = x.Email,
                    dienThoai = x.DienThoai,
                    maVaiTro = x.MaVaiTro
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(
                    "Không tìm thấy nhân viên");
            }

            return Ok(user);
        }

        // Thêm nhân viên
        [HttpPost]
        public async Task<IActionResult> CreateEmployee(
            CreateEmployeeRequest request)
        {
            var user = new NguoiDung
            {
                HoTen = request.HoTen,
                Email = request.Email,
                DienThoai = request.DienThoai,
                MaVaiTro = request.MaVaiTro,
                MatKhau = BCrypt.Net.BCrypt.HashPassword(
                    request.MatKhau),
                TrangThaiHoatDong = true,
                NgayTao = DateTime.Now
            };

            _context.NguoiDung.Add(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Thêm nhân viên thành công"
            });
        }

        // Cập nhật nhân viên
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(
            int id,
            UpdateEmployeeRequest request)
        {
            var user = await _context.NguoiDung
                .FindAsync(id);

            if (user == null)
            {
                return NotFound(
                    "Không tìm thấy nhân viên");
            }

            user.HoTen = request.HoTen;
            user.Email = request.Email;
            user.DienThoai = request.DienThoai;
            user.MaVaiTro = request.MaVaiTro;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật thành công"
            });
        }

        // Cập nhật vai trò
        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(
            int id,
            UpdateRoleRequest request)
        {
            var user = await _context.NguoiDung
                .FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.MaVaiTro = request.MaVaiTro;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // Khóa / mở khóa tài khoản
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            UpdateStatusRequest request)
        {
            var user = await _context.NguoiDung
                .FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.TrangThaiHoatDong =
                request.TrangThaiHoatDong;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
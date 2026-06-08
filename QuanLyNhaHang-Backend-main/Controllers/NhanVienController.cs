using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyNhaHangAPI.Services;

namespace QuanLyNhaHangAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Nhân viên,Admin")] // Lưu ý: Tên Role ở đây phải khớp chính xác với chuỗi lưu trong Database của bạn
    public class NhanVienController : ControllerBase
    {
        private readonly INhanVienService _nhanVienService;

        public NhanVienController(INhanVienService nhanVienService)
        {
            _nhanVienService = nhanVienService;
        }

        [HttpGet("ban-an")]
        public async Task<IActionResult> GetDanhSachBanAn()
        {
            var result = await _nhanVienService.LayDanhSachBanAnAsync();
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("don-hang-moi")]
        public async Task<IActionResult> GetDonHangMoi()
        {
            var result = await _nhanVienService.LayDonHangMoiAsync();
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
    }
}
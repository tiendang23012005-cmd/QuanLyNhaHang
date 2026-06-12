using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHangAPI.Data;
using QuanLyNhaHangAPI.Data.Entities;
using QuanLyNhaHangAPI.Models;

namespace QuanLyNhaHangAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableManagementController : ControllerBase
    {
        private readonly QuanLyNhaHangDbContext _context;

        public TableManagementController(
            QuanLyNhaHangDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách bàn
        [HttpGet]
        public async Task<IActionResult> GetTables()
        {
            var tables = await _context.BanAn
                .Select(x => new
                {
                    x.MaBan,
                    x.SoBan,
                    x.SucChua,
                    x.TrangThaiBan
                })
                .ToListAsync();

            return Ok(tables);
        }

        // Thêm bàn
        [HttpPost]
        public async Task<IActionResult> CreateTable(
            CreateTableRequest request)
        {
            var existedTable = await _context.BanAn
                .AnyAsync(x => x.SoBan == request.SoBan);

            if (existedTable)
            {
                return BadRequest(
                    "Số bàn đã tồn tại");
            }

            var table = new BanAn
            {
                SoBan = request.SoBan,
                SucChua = request.SucChua,
                TrangThaiBan = request.TrangThaiBan
            };

            _context.BanAn.Add(table);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Thêm bàn thành công"
            });
        }

        // Cập nhật bàn
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTable(
            int id,
            UpdateTableRequest request)
        {
            var table = await _context.BanAn
                .FindAsync(id);

            if (table == null)
            {
                return NotFound(
                    "Không tìm thấy bàn");
            }

            table.SoBan = request.SoBan;
            table.SucChua = request.SucChua;
            table.TrangThaiBan =
                request.TrangThaiBan;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật thành công"
            });
        }

        // Xóa bàn
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(
            int id)
        {
            var table = await _context.BanAn
                .FindAsync(id);

            if (table == null)
            {
                return NotFound(
                    "Không tìm thấy bàn");
            }

            _context.BanAn.Remove(table);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Xóa thành công"
            });
        }
    }
}
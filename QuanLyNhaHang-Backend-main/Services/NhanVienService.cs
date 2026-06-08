using Microsoft.EntityFrameworkCore;
using QuanLyNhaHangAPI.Data;
using QuanLyNhaHangAPI.Models;

namespace QuanLyNhaHangAPI.Services
{
    public class NhanVienService : INhanVienService
    {
        private readonly QuanLyNhaHangDbContext _context;

        public NhanVienService(QuanLyNhaHangDbContext context)
        {
            _context = context;
        }

        public async Task<NhanVienApiResponse<List<BanAnDto>>> LayDanhSachBanAnAsync()
        {
            try
            {
                var danhSachBan = await _context.BanAn
                    .AsNoTracking()
                    .Select(b => new BanAnDto
                    {
                        MaBan = b.MaBan,
                        SoBan = b.SoBan,
                        TrangThaiBan = b.TrangThaiBan
                    })
                    .ToListAsync();

                return new NhanVienApiResponse<List<BanAnDto>>
                {
                    IsSuccess = true,
                    Message = "Lay du lieu ban an thanh cong.",
                    Data = danhSachBan
                };
            }
            catch (Exception ex)
            {
                return new NhanVienApiResponse<List<BanAnDto>>
                {
                    IsSuccess = false,
                    Message = $"Loi khi tai so do ban: {ex.Message}"
                };
            }
        }

        public async Task<NhanVienApiResponse<List<DonHangNhanVienDto>>> LayDonHangMoiAsync()
        {
            try
            {
                var danhSachDonHang = await _context.DonHang
                    .Include(d => d.MaBanNavigation)
                    .Where(d => d.TrangThaiDon == "Chờ xác nhận" || d.TrangThaiDon == "Đang chuẩn bị")
                    .OrderByDescending(d => d.NgayTao)
                    .Select(d => new DonHangNhanVienDto
                    {
                        MaDonHang = d.MaDonHang,
                        MaBan = d.MaBan,
                        TenBan = d.MaBanNavigation != null ? d.MaBanNavigation.SoBan : "Mua mang ve",
                        LoaiDonHang = d.LoaiDonHang,
                        NgayTao = d.NgayTao,
                        TongTien = d.TongTien,
                        TrangThaiDon = d.TrangThaiDon
                    })
                    .ToListAsync();

                return new NhanVienApiResponse<List<DonHangNhanVienDto>>
                {
                    IsSuccess = true,
                    Message = "Lay danh sach don hang thanh cong.",
                    Data = danhSachDonHang
                };
            }
            catch (Exception ex)
            {
                return new NhanVienApiResponse<List<DonHangNhanVienDto>>
                {
                    IsSuccess = false,
                    Message = $"Loi khi tai don hang: {ex.Message}"
                };
            }
        }
    }
}
using QuanLyNhaHangAPI.Models;

namespace QuanLyNhaHangAPI.Services
{
    public interface INhanVienService
    {
        Task<NhanVienApiResponse<List<BanAnDto>>> LayDanhSachBanAnAsync();
        Task<NhanVienApiResponse<List<DonHangNhanVienDto>>> LayDonHangMoiAsync();
    }
}
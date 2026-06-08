namespace QuanLyNhaHangAPI.Models
{
    public class BanAnDto
    {
        public int MaBan { get; set; }
        public string SoBan { get; set; } = null!;
        public string? TrangThaiBan { get; set; }
    }

    public class DonHangNhanVienDto
    {
        public int MaDonHang { get; set; }
        public int? MaBan { get; set; }
        public string? TenBan { get; set; }
        public string? LoaiDonHang { get; set; }
        public DateTime? NgayTao { get; set; }
        public decimal? TongTien { get; set; }
        public string? TrangThaiDon { get; set; }
    }

    public class NhanVienApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = null!;
        public T? Data { get; set; }
    }
}

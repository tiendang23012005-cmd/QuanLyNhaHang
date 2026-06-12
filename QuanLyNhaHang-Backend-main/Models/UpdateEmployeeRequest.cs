namespace QuanLyNhaHangAPI.Models
{
    public class UpdateEmployeeRequest
    {
        public string HoTen { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string DienThoai { get; set; } = null!;

        public int MaVaiTro { get; set; }
    }
}
namespace QuanLyNhaHangAPI.Models
{
    public class CreateFoodRequest
    {
        public int MaDanhMuc { get; set; }

        public string TenMonAn { get; set; } = null!;

        public decimal Gia { get; set; }

        public string? MoTa { get; set; }

        public string? HinhAnh { get; set; }

        public bool ConBan { get; set; } = true;
    }

    public class UpdateFoodRequest
    {
        public int MaDanhMuc { get; set; }

        public string TenMonAn { get; set; } = null!;

        public decimal Gia { get; set; }

        public string? MoTa { get; set; }

        public string? HinhAnh { get; set; }

        public bool ConBan { get; set; }
    }
}
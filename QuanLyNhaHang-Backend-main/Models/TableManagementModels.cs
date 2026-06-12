namespace QuanLyNhaHangAPI.Models
{
    public class CreateTableRequest
    {
        public string SoBan { get; set; } = null!;

        public int SucChua { get; set; }

        public string TrangThaiBan { get; set; } = "Trống";
    }

    public class UpdateTableRequest
    {
        public string SoBan { get; set; } = null!;

        public int SucChua { get; set; }

        public string TrangThaiBan { get; set; } = null!;
    }
}
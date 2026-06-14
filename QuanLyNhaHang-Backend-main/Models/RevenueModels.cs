namespace QuanLyNhaHangAPI.Models
{
    public class RevenueSummaryDto
    {
        public decimal TotalRevenue { get; set; }

        public int TotalBills { get; set; }

        public int TotalCustomers { get; set; }

        public decimal YearlyTarget { get; set; } = 500000000;

        public List<MonthlyRevenueItem> MonthlyRevenue { get; set; } = new();

        public List<RevenueDetailItem> RevenueDetails { get; set; } = new();
    }

    public class MonthlyRevenueItem
    {
        public string Month { get; set; } = "";

        public decimal Revenue { get; set; }
    }

    public class RevenueDetailItem
    {
        public string Month { get; set; } = "";

        public decimal Revenue { get; set; }

        public int Bills { get; set; }

        public int Customers { get; set; }
    }
}
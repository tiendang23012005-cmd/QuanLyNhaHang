using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHangAPI.Data;
using QuanLyNhaHangAPI.Models;

namespace QuanLyNhaHangAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenueManagementController : ControllerBase
    {
        private readonly QuanLyNhaHangDbContext _context;

        public RevenueManagementController(
            QuanLyNhaHangDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenue()
        {
            var currentYear = DateTime.Now.Year;

            var orders = await _context.DonHang
                .Include(x => x.MaBanNavigation)
                .Where(x =>
                    x.TrangThaiThanhToan != null &&
                    x.TrangThaiThanhToan.Contains("Đã thanh toán"))
                .ToListAsync();

            var result = new RevenueSummaryDto
            {
                TotalRevenue = orders.Sum(x => x.TongTien ?? 0),

                TotalBills = orders.Count,

                TotalCustomers = orders.Sum(x =>
                    x.MaBanNavigation?.SucChua ?? 0)
            };

            for (int month = 1; month <= 12; month++)
            {
                var monthlyOrders = orders
                    .Where(x =>
                        x.NgayTao.HasValue &&
                        x.NgayTao.Value.Year == currentYear &&
                        x.NgayTao.Value.Month == month)
                    .ToList();

                decimal revenue =
                    monthlyOrders.Sum(x => x.TongTien ?? 0);

                int bills =
                    monthlyOrders.Count;

                int customers =
                    monthlyOrders.Sum(x =>
                        x.MaBanNavigation?.SucChua ?? 0);

                result.MonthlyRevenue.Add(
                    new MonthlyRevenueItem
                    {
                        Month = $"T{month}",
                        Revenue = revenue
                    });

                result.RevenueDetails.Add(
                    new RevenueDetailItem
                    {
                        Month = $"Tháng {month}",
                        Revenue = revenue,
                        Bills = bills,
                        Customers = customers
                    });
            }

            return Ok(result);
        }
    }
}
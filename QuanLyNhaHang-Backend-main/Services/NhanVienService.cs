using Microsoft.EntityFrameworkCore;
using QuanLyNhaHangAPI.Data;
using QuanLyNhaHangAPI.Data.Entities;
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

        // 1. Lấy danh sách bàn — tự đồng bộ trạng thái bàn với đơn hàng thực tế
        public async Task<NhanVienApiResponse<List<BanAnDto>>> LayDanhSachBanAnAsync()
        {
            try
            {
                var danhSachBan = await _context.BanAn.ToListAsync();

                var maBanCoDon = await _context.DonHang
                    .Where(d => d.MaBan != null
                             && d.TrangThaiDon != "Đã hủy"
                             && d.TrangThaiDon != "Hoàn thành"
                             && (d.TrangThaiThanhToan == "Chưa thanh toán" || d.TrangThaiThanhToan == null))
                    .Select(d => d.MaBan!.Value)
                    .Distinct()
                    .ToListAsync();

                bool coThayDoi = false;
                var result = new List<BanAnDto>();

                foreach (var ban in danhSachBan)
                {
                    bool coDon = maBanCoDon.Contains(ban.MaBan);

                    if (coDon && ban.TrangThaiBan != "Đang phục vụ")
                    {
                        ban.TrangThaiBan = "Đang phục vụ";
                        _context.BanAn.Update(ban);
                        coThayDoi = true;
                    }
                    else if (!coDon && ban.TrangThaiBan == "Đang phục vụ")
                    {
                        ban.TrangThaiBan = "Trống";
                        _context.BanAn.Update(ban);
                        coThayDoi = true;
                    }

                    result.Add(new BanAnDto
                    {
                        MaBan = ban.MaBan,
                        SoBan = ban.SoBan,
                        TrangThaiBan = ban.TrangThaiBan
                    });
                }

                if (coThayDoi) await _context.SaveChangesAsync();

                return new NhanVienApiResponse<List<BanAnDto>> { IsSuccess = true, Data = result };
            }
            catch (Exception ex)
            {
                return new NhanVienApiResponse<List<BanAnDto>> { IsSuccess = false, Message = $"Lỗi tải bàn: {ex.Message}" };
            }
        }

        // 2. Lấy đơn hàng mới (chờ xác nhận / đang chuẩn bị)
        public async Task<NhanVienApiResponse<List<DonHangNhanVienDto>>> LayDonHangMoiAsync()
        {
            try
            {
                var data = await _context.DonHang
                    .Include(d => d.MaBanNavigation)
                    .Where(d => d.TrangThaiDon == "Chờ xác nhận" || d.TrangThaiDon == "Đang chuẩn bị")
                    .OrderByDescending(d => d.NgayTao)
                    .Select(d => new DonHangNhanVienDto
                    {
                        MaDonHang = d.MaDonHang,
                        MaBan = d.MaBan,
                        TenBan = d.MaBanNavigation != null ? d.MaBanNavigation.SoBan : "Mang về",
                        LoaiDonHang = d.LoaiDonHang,
                        NgayTao = d.NgayTao,
                        TongTien = d.TongTien,
                        TrangThaiDon = d.TrangThaiDon
                    })
                    .ToListAsync();

                return new NhanVienApiResponse<List<DonHangNhanVienDto>> { IsSuccess = true, Data = data };
            }
            catch (Exception ex)
            {
                return new NhanVienApiResponse<List<DonHangNhanVienDto>> { IsSuccess = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        // 3. Lấy đơn hàng hiện tại theo bàn (maBan = 0 → mang về)
        public async Task<NhanVienApiResponse<DonHangHienTaiDto>> LayDonHangHienTaiTheoBanAsync(int maBan)
        {
            try
            {
                DonHang? donHang;

                if (maBan <= 0)
                {
                    donHang = await _context.DonHang
                        .Include(d => d.ChiTietDonHang).ThenInclude(c => c.MaMonAnNavigation)
                        .Where(d => d.MaBan == null
                                 && d.LoaiDonHang == "Mang về"
                                 && d.TrangThaiDon != "Đã hủy"
                                 && d.TrangThaiDon != "Hoàn thành"
                                 && (d.TrangThaiThanhToan == "Chưa thanh toán" || d.TrangThaiThanhToan == null))
                        .OrderByDescending(d => d.NgayTao)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    donHang = await _context.DonHang
                        .Include(d => d.MaBanNavigation)
                        .Include(d => d.ChiTietDonHang).ThenInclude(c => c.MaMonAnNavigation)
                        .Where(d => d.MaBan == maBan
                                 && d.TrangThaiDon != "Đã hủy"
                                 && d.TrangThaiDon != "Hoàn thành"
                                 && (d.TrangThaiThanhToan == "Chưa thanh toán" || d.TrangThaiThanhToan == null))
                        .OrderByDescending(d => d.NgayTao)
                        .FirstOrDefaultAsync();
                }

                if (donHang == null)
                    return new NhanVienApiResponse<DonHangHienTaiDto> { IsSuccess = false, Message = "Bàn này chưa có đơn hàng." };

                var dto = new DonHangHienTaiDto
                {
                    MaDonHang = donHang.MaDonHang,
                    MaBan = donHang.MaBan,
                    SoBan = donHang.MaBan.HasValue ? donHang.MaBanNavigation?.SoBan : "Mang về",
                    TongTien = donHang.TongTien ?? 0,
                    TrangThaiDon = donHang.TrangThaiDon,
                    TrangThaiThanhToan = donHang.TrangThaiThanhToan,
                    ChiTietDonHangs = donHang.ChiTietDonHang.Select(c => new ChiTietDonHangDto
                    {
                        MaChiTiet = c.MaChiTiet,
                        MaMonAn = c.MaMonAn,
                        TenMonAn = c.MaMonAnNavigation?.TenMonAn ?? "Món đã xóa",
                        SoLuong = c.SoLuong,
                        GiaLucDat = c.GiaLucDat,
                        GhiChu = c.GhiChu,
                        TrangThaiBep = c.TrangThaiBep
                    }).ToList()
                };

                return new NhanVienApiResponse<DonHangHienTaiDto> { IsSuccess = true, Data = dto };
            }
            catch (Exception ex)
            {
                return new NhanVienApiResponse<DonHangHienTaiDto> { IsSuccess = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        // 4. Cập nhật số lượng món (soLuongMoi = 0 → xóa dòng đó)
        public async Task<NhanVienApiResponse<bool>> CapNhatSoLuongMonAsync(int maChiTiet, int soLuongMoi)
        {
            try
            {
                var chiTiet = await _context.ChiTietDonHang.FindAsync(maChiTiet);
                if (chiTiet == null)
                    return new NhanVienApiResponse<bool> { IsSuccess = false, Message = "Không tìm thấy chi tiết đơn hàng." };

                int maDonHang = chiTiet.MaDonHang;

                if (soLuongMoi <= 0)
                    _context.ChiTietDonHang.Remove(chiTiet);
                else
                {
                    chiTiet.SoLuong = soLuongMoi;
                    _context.ChiTietDonHang.Update(chiTiet);
                }

                await _context.SaveChangesAsync();
                await TinhLaiTongTienAsync(maDonHang);

                return new NhanVienApiResponse<bool> { IsSuccess = true, Data = true };
            }
            catch (Exception ex)
            {
                return new NhanVienApiResponse<bool> { IsSuccess = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        // 5. Thêm món vào đơn (tự tạo đơn mới nếu chưa có)
        public async Task<NhanVienApiResponse<int>> ThemMonVaoDonHienTaiAsync(ThemMonVaoDonRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var monAn = await _context.MonAn.FindAsync(request.MaMonAn);
                if (monAn == null)
                    return new NhanVienApiResponse<int> { IsSuccess = false, Message = "Món ăn không tồn tại." };

                int maDonHang = 0;

                // Ưu tiên 1: tìm theo mã đơn hàng
                if (request.MaDonHang > 0)
                {
                    var don = await _context.DonHang.FindAsync(request.MaDonHang);
                    if (don != null && don.TrangThaiDon != "Đã hủy" && don.TrangThaiDon != "Hoàn thành")
                        maDonHang = don.MaDonHang;
                }

                // Ưu tiên 2: tìm theo bàn
                if (maDonHang == 0 && request.MaBan > 0)
                {
                    var don = await _context.DonHang
                        .Where(d => d.MaBan == request.MaBan
                                 && d.TrangThaiDon != "Đã hủy"
                                 && d.TrangThaiDon != "Hoàn thành"
                                 && (d.TrangThaiThanhToan == "Chưa thanh toán" || d.TrangThaiThanhToan == null))
                        .OrderByDescending(d => d.NgayTao)
                        .FirstOrDefaultAsync();
                    if (don != null) maDonHang = don.MaDonHang;
                }

                // Tạo đơn mới nếu chưa có
                if (maDonHang == 0)
                {
                    var donMoi = new DonHang
                    {
                        MaBan = request.MaBan > 0 ? request.MaBan : null,
                        LoaiDonHang = request.MaBan > 0 ? "Tại quán" : "Mang về",
                        TrangThaiDon = "Chờ Xác Nhận",
                        TrangThaiThanhToan = "Chưa thanh toán",
                        PhuongThucThanhToan = "Tiền mặt",
                        NgayTao = DateTime.Now,
                        TongTien = 0
                    };
                    await _context.DonHang.AddAsync(donMoi);
                    await _context.SaveChangesAsync();
                    maDonHang = donMoi.MaDonHang;

                    if (request.MaBan > 0)
                    {
                        var ban = await _context.BanAn.FindAsync(request.MaBan);
                        if (ban != null) { ban.TrangThaiBan = "Đang phục vụ"; _context.BanAn.Update(ban); }
                    }
                }

                // Cộng dồn hoặc thêm mới chi tiết món
                var chiTiet = await _context.ChiTietDonHang
                    .FirstOrDefaultAsync(c => c.MaDonHang == maDonHang && c.MaMonAn == request.MaMonAn);

                if (chiTiet != null)
                {
                    chiTiet.SoLuong += request.SoLuong;
                    _context.ChiTietDonHang.Update(chiTiet);
                }
                else
                {
                    await _context.ChiTietDonHang.AddAsync(new ChiTietDonHang
                    {
                        MaDonHang = maDonHang,
                        MaMonAn = request.MaMonAn,
                        SoLuong = request.SoLuong,
                        GiaLucDat = monAn.Gia,
                        TrangThaiBep = "Chờ chế biến"
                    });
                }

                await _context.SaveChangesAsync();
                await TinhLaiTongTienAsync(maDonHang);
                await transaction.CommitAsync();

                return new NhanVienApiResponse<int> { IsSuccess = true, Data = maDonHang };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new NhanVienApiResponse<int> { IsSuccess = false, Message = ex.InnerException?.Message ?? ex.Message };
            }
        }

        // 6. Xác nhận thanh toán — đổi trạng thái đơn và giải phóng bàn
        public async Task<NhanVienApiResponse<bool>> XacNhanThanhToanAsync(int maDonHang)
        {
            try
            {
                var donHang = await _context.DonHang.FindAsync(maDonHang);
                if (donHang == null)
                    return new NhanVienApiResponse<bool> { IsSuccess = false, Message = "Không tìm thấy đơn hàng." };

                donHang.TrangThaiThanhToan = "Đã thanh toán online";
                donHang.TrangThaiDon = "Hoàn thành";
                _context.DonHang.Update(donHang);

                if (donHang.MaBan != null)
                {
                    var ban = await _context.BanAn.FindAsync(donHang.MaBan);
                    if (ban != null) { ban.TrangThaiBan = "Trống"; _context.BanAn.Update(ban); }
                }

                await _context.SaveChangesAsync();
                return new NhanVienApiResponse<bool> { IsSuccess = true, Data = true };
            }
            catch (Exception ex)
            {
                return new NhanVienApiResponse<bool> { IsSuccess = false, Message = ex.InnerException?.Message ?? ex.Message };
            }
        }

        // 7. Hủy đơn hàng — chỉ giải phóng bàn khi không còn đơn khác
        public async Task<NhanVienApiResponse<bool>> HuyDonHangAsync(int maDonHang)
        {
            try
            {
                var donHang = await _context.DonHang.FindAsync(maDonHang);
                if (donHang == null)
                    return new NhanVienApiResponse<bool> { IsSuccess = false, Message = "Không tìm thấy đơn hàng." };

                donHang.TrangThaiDon = "Đã hủy";
                _context.DonHang.Update(donHang);

                if (donHang.MaBan != null)
                {
                    bool conDonKhac = await _context.DonHang.AnyAsync(d =>
                        d.MaBan == donHang.MaBan
                        && d.MaDonHang != maDonHang
                        && d.TrangThaiDon != "Đã hủy"
                        && d.TrangThaiDon != "Hoàn thành"
                        && (d.TrangThaiThanhToan == "Chưa thanh toán" || d.TrangThaiThanhToan == null));

                    if (!conDonKhac)
                    {
                        var ban = await _context.BanAn.FindAsync(donHang.MaBan);
                        if (ban != null) { ban.TrangThaiBan = "Trống"; _context.BanAn.Update(ban); }
                    }
                }

                await _context.SaveChangesAsync();
                return new NhanVienApiResponse<bool> { IsSuccess = true, Data = true };
            }
            catch (Exception ex)
            {
                return new NhanVienApiResponse<bool> { IsSuccess = false, Message = ex.InnerException?.Message ?? ex.Message };
            }
        }

        // Helper: tính lại tổng tiền sau mỗi thao tác
        private async Task TinhLaiTongTienAsync(int maDonHang)
        {
            var donHang = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

            if (donHang != null)
            {
                donHang.TongTien = donHang.ChiTietDonHang.Sum(c => c.SoLuong * c.GiaLucDat);
                _context.DonHang.Update(donHang);
                await _context.SaveChangesAsync();
            }
        }
    }
}

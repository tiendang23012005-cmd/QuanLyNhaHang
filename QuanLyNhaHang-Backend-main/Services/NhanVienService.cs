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

        // 1. Lấy danh sách bàn ăn
        public async Task<NhanVienApiResponse<List<BanAnDto>>> LayDanhSachBanAnAsync()
        {
            try
            {
                // 1. Lấy danh sách tất cả các bàn
                var danhSachBan = await _context.BanAn.ToListAsync();

                // 2. Lấy danh sách các MaBan thực sự đang có đơn hàng chưa thanh toán/đang xử lý
                var maBanDangCoDonThucTe = await _context.DonHang
                    .Where(d => d.MaBan != null
                             && d.TrangThaiDon != "Đã hủy"
                             && d.TrangThaiDon != "Hoàn thành"
                             && (d.TrangThaiThanhToan == "Chưa thanh toán" || d.TrangThaiThanhToan == null))
                    .Select(d => d.MaBan!.Value)
                    .Distinct()
                    .ToListAsync();

                var resultDto = new List<BanAnDto>();
                bool coSuaDoiDuLieu = false;

                foreach (var ban in danhSachBan)
                {
                    bool thucTeCoDon = maBanDangCoDonThucTe.Contains(ban.MaBan);

                    // TỰ ĐỘNG ĐỒNG BỘ: Sửa lỗi hiển thị sai trạng thái bàn ăn
                    if (thucTeCoDon && ban.TrangThaiBan != "Đang phục vụ")
                    {
                        ban.TrangThaiBan = "Đang phục vụ";
                        _context.BanAn.Update(ban);
                        coSuaDoiDuLieu = true;
                    }
                    else if (!thucTeCoDon && ban.TrangThaiBan == "Đang phục vụ")
                    {
                        ban.TrangThaiBan = "Trống";
                        _context.BanAn.Update(ban);
                        coSuaDoiDuLieu = true;
                    }

                    // Map sang DTO để trả về giao diện Angular
                    resultDto.Add(new BanAnDto
                    {
                        MaBan = ban.MaBan,
                        SoBan = ban.SoBan,
                        TrangThaiBan = ban.TrangThaiBan // Trạng thái đã được chuẩn hóa chuẩn 100%
                                                        // Nếu DTO của bạn có thêm trường gì (Vị trí, Sức chứa...), hãy bổ sung ở đây
                    });
                }

                // Nếu có bàn nào bị lệch trạng thái, lưu lại vào DB luôn để dọn rác
                if (coSuaDoiDuLieu)
                {
                    await _context.SaveChangesAsync();
                }

                return new NhanVienApiResponse<List<BanAnDto>> { IsSuccess = true, Message = "Tải danh sách bàn thành công", Data = resultDto };
            }
            catch (Exception ex)
            {
                return new NhanVienApiResponse<List<BanAnDto>> { IsSuccess = false, Message = $"Lỗi tải bàn: {ex.Message}" };
            }
        }

        // 2. Lấy đơn hàng mới
        public async Task<NhanVienApiResponse<List<DonHangNhanVienDto>>> LayDonHangMoiAsync()
        {
            try
            {
                var donHangs = await _context.DonHang
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

                return new NhanVienApiResponse<List<DonHangNhanVienDto>> { IsSuccess = true, Message = "Thành công", Data = donHangs };
            }
            catch (Exception ex)
            {
                return new NhanVienApiResponse<List<DonHangNhanVienDto>> { IsSuccess = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        // 3. Lấy đơn hàng hiện tại theo bàn
        public async Task<NhanVienApiResponse<DonHangHienTaiDto>> LayDonHangHienTaiTheoBanAsync(int maBan)
        {
            try
            {
                DonHang? donHang = null;
                if (maBan <= 0)
                {
                    donHang = await _context.DonHang
                        .Include(d => d.ChiTietDonHang).ThenInclude(c => c.MaMonAnNavigation)
                        .Where(d => d.MaBan == null && d.LoaiDonHang == "Mang về"
                                 && (d.TrangThaiThanhToan == "Chưa thanh toán" || d.TrangThaiThanhToan == null)
                                 && d.TrangThaiDon != "Đã hủy" && d.TrangThaiDon != "Hoàn thành") // Sửa lỗi lấy nhầm đơn cũ
                        .OrderByDescending(d => d.NgayTao)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    donHang = await _context.DonHang
                        .Include(d => d.MaBanNavigation)
                        .Include(d => d.ChiTietDonHang).ThenInclude(c => c.MaMonAnNavigation)
                        .Where(d => d.MaBan == maBan
                                 && (d.TrangThaiThanhToan == "Chưa thanh toán" || d.TrangThaiThanhToan == null)
                                 && d.TrangThaiDon != "Đã hủy" && d.TrangThaiDon != "Hoàn thành") // Sửa lỗi lấy nhầm đơn cũ
                        .OrderByDescending(d => d.NgayTao)
                        .FirstOrDefaultAsync();
                }

                if (donHang == null)
                {
                    return new NhanVienApiResponse<DonHangHienTaiDto> { IsSuccess = false, Message = "Bàn này hiện chưa có hóa đơn đang xử lý." };
                }

                // ... (Đoạn map sang DonHangHienTaiDto giữ nguyên như cũ của bạn)
                var donHangDto = new DonHangHienTaiDto
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
                        TenMonAn = c.MaMonAnNavigation != null ? c.MaMonAnNavigation.TenMonAn : "Món đã xóa",
                        SoLuong = c.SoLuong,
                        GiaLucDat = c.GiaLucDat,
                        GhiChu = c.GhiChu,
                        TrangThaiBep = c.TrangThaiBep
                    }).ToList()
                };
                return new NhanVienApiResponse<DonHangHienTaiDto> { IsSuccess = true, Message = "Thành công", Data = donHangDto };
            }
            catch (Exception ex)
            {
                return new NhanVienApiResponse<DonHangHienTaiDto> { IsSuccess = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        // 4. Cập nhật số lượng món
        public async Task<NhanVienApiResponse<bool>> CapNhatSoLuongMonAsync(int maChiTiet, int soLuongMoi)
        {
            try
            {
                var chiTiet = await _context.ChiTietDonHang.FindAsync(maChiTiet);
                if (chiTiet == null) return new NhanVienApiResponse<bool> { IsSuccess = false, Message = "Chi tiết đơn hàng không tồn tại." };

                int maDonHang = chiTiet.MaDonHang; // ĐÃ SỬA: Bỏ ?? 0 vì thuộc tính thực thể là kiểu 'int'

                if (soLuongMoi <= 0)
                {
                    _context.ChiTietDonHang.Remove(chiTiet);
                }
                else
                {
                    chiTiet.SoLuong = soLuongMoi;
                    _context.ChiTietDonHang.Update(chiTiet);
                }

                await _context.SaveChangesAsync();
                await TinhLaiTongTienDonHangAsync(maDonHang);

                return new NhanVienApiResponse<bool> { IsSuccess = true, Message = "Cập nhật thành công.", Data = true };
            }
            catch (Exception ex)
            {
                return new NhanVienApiResponse<bool> { IsSuccess = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        // 5. Thêm món vào đơn hiện tại (Sửa lỗi gọi thêm món bị tách đơn)
        public async Task<NhanVienApiResponse<int>> ThemMonVaoDonHienTaiAsync(ThemMonVaoDonRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var monAn = await _context.MonAn.FindAsync(request.MaMonAn);
                if (monAn == null) return new NhanVienApiResponse<int> { IsSuccess = false, Message = "Món ăn không tồn tại." };

                int maDonHangChinhThuc = 0;

                // BƯỚC 1: ƯU TIÊN GỘP THEO MÃ ĐƠN HÀNG (> 0 nghĩa là đã có mã đơn)
                if (request.MaDonHang > 0)
                {
                    var donHangTheoId = await _context.DonHang.FindAsync(request.MaDonHang);
                    if (donHangTheoId != null && donHangTheoId.TrangThaiDon != "Đã hủy" && donHangTheoId.TrangThaiDon != "Hoàn thành")
                    {
                        maDonHangChinhThuc = donHangTheoId.MaDonHang;
                    }
                }

                // BƯỚC 2: NẾU KHÔNG CÓ MÃ ĐƠN HÀNG, TÌM THEO ID BÀN (> 0 nghĩa là đơn tại bàn)
                if (maDonHangChinhThuc == 0 && request.MaBan > 0)
                {
                    var donHangHienCo = await _context.DonHang
                        .Where(d => d.MaBan == request.MaBan
                                 && d.TrangThaiDon != "Đã hủy"
                                 && d.TrangThaiDon != "Hoàn thành"
                                 && (d.TrangThaiThanhToan == "Chưa thanh toán" || d.TrangThaiThanhToan == null))
                        .OrderByDescending(d => d.NgayTao)
                        .FirstOrDefaultAsync();

                    if (donHangHienCo != null)
                    {
                        maDonHangChinhThuc = donHangHienCo.MaDonHang;
                    }
                }

                // BƯỚC 3: TẠO ĐƠN MỚI NẾU KHÔNG TÌM THẤY ĐƠN CŨ (Cả đơn bàn lẫn mang về)
                if (maDonHangChinhThuc == 0)
                {
                    var donHangMoi = new DonHang
                    {
                        MaBan = (request.MaBan > 0) ? request.MaBan : null,
                        LoaiDonHang = (request.MaBan > 0) ? "Tại quán" : "Mang về",
                        TrangThaiDon = "Chờ Xác Nhận",
                        TrangThaiThanhToan = "Chưa thanh toán",
                        PhuongThucThanhToan = "Tiền mặt",
                        NgayTao = DateTime.Now,
                        TongTien = 0
                    };
                    await _context.DonHang.AddAsync(donHangMoi);
                    await _context.SaveChangesAsync();
                    maDonHangChinhThuc = donHangMoi.MaDonHang;

                    // Cập nhật trạng thái bàn ăn sang Đang phục vụ
                    if (request.MaBan > 0)
                    {
                        var banAn = await _context.BanAn.FindAsync(request.MaBan);
                        if (banAn != null)
                        {
                            banAn.TrangThaiBan = "Đang phục vụ";
                            _context.BanAn.Update(banAn);
                        }
                    }
                }

                // BƯỚC 4: XỬ LÝ CỘNG DỒN MÓN ĂN
                var chiTietTonTai = await _context.ChiTietDonHang
                    .FirstOrDefaultAsync(c => c.MaDonHang == maDonHangChinhThuc && c.MaMonAn == request.MaMonAn);

                if (chiTietTonTai != null)
                {
                    chiTietTonTai.SoLuong += request.SoLuong;
                    _context.ChiTietDonHang.Update(chiTietTonTai);
                }
                else
                {
                    var chiTietMoi = new ChiTietDonHang
                    {
                        MaDonHang = maDonHangChinhThuc,
                        MaMonAn = request.MaMonAn,
                        SoLuong = request.SoLuong,
                        GiaLucDat = monAn.Gia,
                        TrangThaiBep = "Chờ chế biến"
                    };
                    await _context.ChiTietDonHang.AddAsync(chiTietMoi);
                }

                await _context.SaveChangesAsync();
                await TinhLaiTongTienDonHangAsync(maDonHangChinhThuc);

                await transaction.CommitAsync();
                return new NhanVienApiResponse<int> { IsSuccess = true, Message = "Cập nhật đơn hàng thành công.", Data = maDonHangChinhThuc };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // LẤY LỖI THỰC SỰ TỪ CƠ SỞ DỮ LIỆU (INNER EXCEPTION)
                var detailedError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                return new NhanVienApiResponse<int>
                {
                    IsSuccess = false,
                    Message = $"Lỗi CSDL chi tiết: {detailedError}"
                };
            }
        }

        // 6. Xác nhận thanh toán và giải phóng bàn
        public async Task<NhanVienApiResponse<bool>> XacNhanThanhToanAsync(int maDonHang)
        {
            try
            {
                var donHang = await _context.DonHang.FindAsync(maDonHang);
                if (donHang == null) return new NhanVienApiResponse<bool> { IsSuccess = false, Message = "Không tìm thấy đơn hàng." };

                donHang.TrangThaiThanhToan = "Đã thanh toán online";
                donHang.TrangThaiDon = "Hoàn thành";
                _context.DonHang.Update(donHang);

                if (donHang.MaBan != null)
                {
                    var banAn = await _context.BanAn.FindAsync(donHang.MaBan);
                    if (banAn != null)
                    {
                        banAn.TrangThaiBan = "Trống";
                        _context.BanAn.Update(banAn);
                    }
                }

                await _context.SaveChangesAsync();
                return new NhanVienApiResponse<bool> { IsSuccess = true, Message = "Thanh toán thành công.", Data = true };
            }
            catch (Exception ex)
            {
                return new NhanVienApiResponse<bool> { IsSuccess = false, Message = $"Lỗi: {ex.InnerException?.Message ?? ex.Message}" };
            }
        }

        // --- Hàm helper tính tổng tiền ---
        private async Task TinhLaiTongTienDonHangAsync(int maDonHang)
        {
            var donHang = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

            if (donHang != null)
            {
                // ĐÃ SỬA: Loại bỏ toàn bộ toán tử ?? thừa kế từ kiểu dữ liệu gốc nguyên bản
                donHang.TongTien = donHang.ChiTietDonHang.Sum(c => c.SoLuong * c.GiaLucDat);
                _context.DonHang.Update(donHang);
                await _context.SaveChangesAsync();
            }
        }

        // BỔ SUNG: Hàm Hủy Đơn Hàng
        public async Task<NhanVienApiResponse<bool>> HuyDonHangAsync(int maDonHang)
        {
            try
            {
                var donHang = await _context.DonHang.FindAsync(maDonHang);
                if (donHang == null) return new NhanVienApiResponse<bool> { IsSuccess = false, Message = "Không tìm thấy đơn hàng." };

                donHang.TrangThaiDon = "Đã hủy";
                // ĐÃ XÓA dòng cập nhật TrangThaiThanhToan để tránh vi phạm cấu trúc Database của bạn

                _context.DonHang.Update(donHang);

                if (donHang.MaBan != null)
                {
                    var conDonKhac = await _context.DonHang.AnyAsync(d => d.MaBan == donHang.MaBan && d.MaDonHang != maDonHang
                                                                       && (d.TrangThaiThanhToan == "Chưa thanh toán" || d.TrangThaiThanhToan == null)
                                                                       && d.TrangThaiDon != "Đã hủy" && d.TrangThaiDon != "Hoàn thành");
                    if (!conDonKhac)
                    {
                        var banAn = await _context.BanAn.FindAsync(donHang.MaBan);
                        if (banAn != null)
                        {
                            banAn.TrangThaiBan = "Trống";
                            _context.BanAn.Update(banAn);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return new NhanVienApiResponse<bool> { IsSuccess = true, Message = "Hủy đơn hàng thành công.", Data = true };
            }
            catch (Exception ex)
            {
                // Hiển thị lỗi gốc (InnerException) nếu có lỗi từ Database
                return new NhanVienApiResponse<bool> { IsSuccess = false, Message = $"Lỗi DB: {ex.InnerException?.Message ?? ex.Message}" };
            }
        }
    }
}
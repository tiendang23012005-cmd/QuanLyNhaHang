import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NhanVienApiResponse, BanAnDto, DonHangNhanVienDto, DonHangHienTaiDto } from '../models/nhan-vien.model';

@Injectable({
  providedIn: 'root'
})
export class NhanVienService {
  private apiUrl = 'https://localhost:7043/api/nhanvien';

  constructor(private http: HttpClient) {}

  // Lấy sơ đồ bàn tại quán
  getDanhSachBan(): Observable<NhanVienApiResponse<BanAnDto[]>> {
    return this.http.get<NhanVienApiResponse<BanAnDto[]>>(`${this.apiUrl}/ban-an`);
  }

  // Xem danh sách hóa đơn mới
  getDonHangMoi(): Observable<NhanVienApiResponse<DonHangNhanVienDto[]>> {
    return this.http.get<NhanVienApiResponse<DonHangNhanVienDto[]>>(`${this.apiUrl}/don-hang-moi`);
  }

  // Chi tiết hóa đơn hiện tại theo mã bàn (Nếu maBan = 0 => Lấy đơn Mang về)
  getDonHangTheoBan(maBan: number): Observable<DonHangHienTaiDto> {
    return this.http.get<DonHangHienTaiDto>(`${this.apiUrl}/ban/${maBan}/don-hang`);
  }

  // Tăng/giảm hoặc xóa món trực tiếp
  capNhatSoLuongMon(maChiTiet: number, soLuongMoi: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/chi-tiet/${maChiTiet}/so-luong`, { soLuongMoi });
  }

  // Thêm món mới vào hóa đơn (Hỗ trợ truyền maBan lên để Backend tự động tạo đơn nếu bàn trống)
  themMonVaoDonHienTai(maDonHang: number, maMonAn: number, soLuong: number, maBan?: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/don-hang/them-mon`, { maDonHang, maMonAn, soLuong, maBan });
  }

  // Thanh toán hóa đơn và reset trạng thái bàn
  xacNhanThanhToan(maDonHang: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/don-hang/${maDonHang}/thanh-toan`, {});
  }

  huyDonHang(maDonHang: number) {
    return this.http.put<any>(`${this.apiUrl}/don-hang/${maDonHang}/huy`, {});
  }
}
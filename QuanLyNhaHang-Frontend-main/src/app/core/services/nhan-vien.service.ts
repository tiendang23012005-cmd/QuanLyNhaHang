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

  // Lấy danh sách sơ đồ bàn (Đã có)
  getDanhSachBan(): Observable<NhanVienApiResponse<BanAnDto[]>> {
    return this.http.get<NhanVienApiResponse<BanAnDto[]>>(`${this.apiUrl}/ban-an`);
  }

  // Lấy đơn hàng mới (Đã có)
  getDonHangMoi(): Observable<NhanVienApiResponse<DonHangNhanVienDto[]>> {
    return this.http.get<NhanVienApiResponse<DonHangNhanVienDto[]>>(`${this.apiUrl}/don-hang-moi`);
  }

  // ==========================================
  // CÁC HÀM MỚI CHO POS
  // ==========================================

  // 1. Lấy chi tiết đơn hàng của 1 bàn
  getDonHangTheoBan(maBan: number): Observable<DonHangHienTaiDto> {
    return this.http.get<DonHangHienTaiDto>(`${this.apiUrl}/ban/${maBan}/don-hang`);
  }

  // 2. Cập nhật số lượng món
  capNhatSoLuongMon(maChiTiet: number, soLuongMoi: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/chi-tiet/${maChiTiet}/so-luong`, { soLuongMoi });
  }

  // 3. Thêm món mới vào đơn
  themMonVaoDon(maDonHang: number, maMonAn: number, soLuong: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/don-hang/them-mon`, { maDonHang, maMonAn, soLuong });
  }

  // 4. Thanh toán đơn hàng
  xacNhanThanhToan(maDonHang: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/don-hang/${maDonHang}/thanh-toan`, {});
  }
}
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NhanVienApiResponse, BanAnDto, DonHangNhanVienDto } from '../models/nhan-vien.model';

@Injectable({
  providedIn: 'root'
})
export class NhanVienService {
  private apiUrl = 'https://localhost:7043/api/nhanvien';

  constructor(private http: HttpClient) {}

  getDanhSachBan(): Observable<NhanVienApiResponse<BanAnDto[]>> {
    return this.http.get<NhanVienApiResponse<BanAnDto[]>>(`${this.apiUrl}/ban-an`);
  }

  getDonHangMoi(): Observable<NhanVienApiResponse<DonHangNhanVienDto[]>> {
    return this.http.get<NhanVienApiResponse<DonHangNhanVienDto[]>>(`${this.apiUrl}/don-hang-moi`);
  }
}
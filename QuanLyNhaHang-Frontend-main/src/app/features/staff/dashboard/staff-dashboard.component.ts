import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription, interval } from 'rxjs';
import { NhanVienService } from '../../../core/services/nhan-vien.service';
import { BanAnDto, DonHangHienTaiDto, MonAn } from '../../../core/models/nhan-vien.model';
import { HttpClient } from '@angular/common/http'; // Để gọi tạm API Món ăn

@Component({
  selector: 'app-staff-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './staff-dashboard.component.html',
  styleUrls: ['./staff-dashboard.component.css']
})
export class StaffDashboardComponent implements OnInit, OnDestroy {
  danhSachBan: BanAnDto[] = [];
  banDangChon: BanAnDto | null = null;
  donHangHienTai: DonHangHienTaiDto | null = null;
  
  // State cho Modal Thêm món
  hienThiModalThemMon = false;
  danhSachMonAn: MonAn[] = []; 
  
  isLoading: boolean = true;
  private pollingSub!: Subscription;
  private nhanVienService = inject(NhanVienService);
  private cdr = inject(ChangeDetectorRef);
  private http = inject(HttpClient); // Inject tạm để gọi danh sách món ăn

  ngOnInit(): void {
    this.loadDanhSachBan();
    this.loadDanhSachMonAn(); // Tải sẵn menu để dùng cho popup thêm món

    // Cập nhật sơ đồ bàn mỗi 15 giây
    this.pollingSub = interval(15000).subscribe(() => {
      this.loadDanhSachBan();
    });
  }

  loadDanhSachBan(): void {
    this.nhanVienService.getDanhSachBan().subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.danhSachBan = res.data;
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      }
    });
  }

  // Tải danh sách menu từ Backend (Giả sử bạn có Endpoint GET /api/MonAn)
  loadDanhSachMonAn(): void {
    this.http.get<any>('https://localhost:7043/api/MonAn').subscribe({
      next: (res) => {
        // Tuỳ thuộc cấu trúc trả về của MonAnController
        this.danhSachMonAn = res.data || res; 
      }
    });
  }

  // KHI NHÂN VIÊN CLICK VÀO 1 BÀN TRÊN SƠ ĐỒ
  chonBan(ban: BanAnDto): void {
    this.banDangChon = ban;
    this.donHangHienTai = null;

    if (ban.trangThaiBan === 'Đang phục vụ' || ban.trangThaiBan === 'Đã đặt') {
      this.nhanVienService.getDonHangTheoBan(ban.maBan).subscribe({
        next: (donHang) => {
          this.donHangHienTai = donHang;
          this.cdr.detectChanges();
        },
        error: () => {
          this.donHangHienTai = null; // Bàn chưa có đơn hàng hoặc lỗi
          this.cdr.detectChanges();
        }
      });
    }
  }

  // THAY ĐỔI SỐ LƯỢNG MÓN
  thayDoiSoLuong(maChiTiet: number, soLuongHienTai: number, thayDoi: number): void {
    const soLuongMoi = soLuongHienTai + thayDoi;
    if (soLuongMoi < 0) return;

    this.nhanVienService.capNhatSoLuongMon(maChiTiet, soLuongMoi).subscribe({
      next: () => {
        // Tải lại chi tiết đơn hàng sau khi cập nhật thành công
        if (this.banDangChon) {
          this.chonBan(this.banDangChon);
        }
      }
    });
  }

  // XỬ LÝ THANH TOÁN
  thanhToan(): void {
    if (!this.donHangHienTai) return;
    
    if (confirm(`Xác nhận thanh toán đơn hàng cho ${this.banDangChon?.soBan}?`)) {
      this.nhanVienService.xacNhanThanhToan(this.donHangHienTai.maDonHang).subscribe({
        next: () => {
          alert('Thanh toán thành công!');
          this.donHangHienTai = null;
          this.loadDanhSachBan(); // Làm mới để cập nhật trạng thái bàn về Trống
        }
      });
    }
  }

  // MỞ / ĐÓNG MODAL THÊM MÓN
  moModalThemMon(): void {
    this.hienThiModalThemMon = true;
  }
  
  dongModalThemMon(): void {
    this.hienThiModalThemMon = false;
  }

  // XÁC NHẬN THÊM MÓN VÀO ĐƠN
  themMonVaoDon(mon: MonAn): void {
    if (!this.donHangHienTai) return;

    // Mặc định thêm số lượng 1 mỗi lần bấm
    this.nhanVienService.themMonVaoDon(this.donHangHienTai.maDonHang, mon.maMonAn, 1).subscribe({
      next: () => {
        this.dongModalThemMon();
        if (this.banDangChon) this.chonBan(this.banDangChon); // Tải lại đơn hàng
      }
    });
  }

  ngOnDestroy(): void {
    if (this.pollingSub) this.pollingSub.unsubscribe();
  }
}
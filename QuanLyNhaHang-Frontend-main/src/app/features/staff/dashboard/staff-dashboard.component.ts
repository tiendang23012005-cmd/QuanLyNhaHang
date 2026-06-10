import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router'; // THÊM DÒNG NÀY
import { Subscription, interval } from 'rxjs';
import { NhanVienService } from '../../../core/services/nhan-vien.service';
import { BanAnDto, DonHangHienTaiDto, MonAn } from '../../../core/models/nhan-vien.model';

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
  
  hienThiModalThemMon = false;
  danhSachMonAn: MonAn[] = [];
  isLoading = true;
  // Thêm vào vùng khai báo thuộc tính State của Component (Dưới biến hienThiModalThemMon)
  hienThiModalIn = false;                  // Bật/tắt màn hình xem trước hóa đơn
  loaiIn: 'tam-tinh' | 'hoa-don' = 'tam-tinh'; // Phân biệt tiêu đề hóa đơn
  hienThiModalXacNhanThanhToan = false;    // Popup xác nhận bấm Thanh toán
  hienThiModalHoiInSauThanhToan = false;   // Popup hỏi in hóa đơn sau khi API thành công
  ngayInHienTai: Date = new Date();        // Thời gian in hóa đơn công bố
  
  private pollingSub!: Subscription;
  private nhanVienService = inject(NhanVienService);
  private cdr = inject(ChangeDetectorRef);
  private http = inject(HttpClient);
  private router = inject(Router); // THÊM INJECT ROUTER Ở ĐÂY

  ngOnInit(): void {
    this.loadDanhSachBan();
    this.loadDanhSachMonAn();
    
    this.pollingSub = interval(15000).subscribe(() => {
      this.loadDanhSachBan();
      if (this.banDangChon) {
        this.refreshDonHangHienTai(this.banDangChon.maBan);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.pollingSub) this.pollingSub.unsubscribe();
  }

  loadDanhSachBan(): void {
    this.nhanVienService.getDanhSachBan().subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.danhSachBan = res.data;
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      },
      error: () => { this.isLoading = false; }
    });
  }

  loadDanhSachMonAn(): void {
    this.http.get<any>('https://localhost:7043/api/MonAn').subscribe({
      next: (res) => {
        this.danhSachMonAn = res.data || res;
        this.cdr.detectChanges();
      }
    });
  }

  chonBan(ban: BanAnDto): void {
    this.banDangChon = ban;
    this.refreshDonHangHienTai(ban.maBan);
  }

  chonMangVe(): void {
    this.banDangChon = { maBan: 0, soBan: 'Đơn Mang Về', trangThaiBan: 'Mang về' };
    this.refreshDonHangHienTai(0);
  }

  refreshDonHangHienTai(maBan: number): void {
    this.nhanVienService.getDonHangTheoBan(maBan).subscribe({
      next: (res) => {
        this.donHangHienTai = res;
        this.cdr.detectChanges();
      },
      error: () => {
        this.donHangHienTai = null;
        this.cdr.detectChanges();
      }
    });
  }

  thayDoiSoLuong(maChiTiet: number, soLuongHienTai: number, thayDoi: number): void {
    const soLuongMoi = soLuongHienTai + thayDoi;
    if (soLuongMoi < 0) return;
    
    this.nhanVienService.capNhatSoLuongMon(maChiTiet, soLuongMoi).subscribe({
      next: () => {
        if (this.banDangChon) this.refreshDonHangHienTai(this.banDangChon.maBan);
      }
    });
  }

  themMonVaoDon(mon: MonAn): void {
    const maDonHang = this.donHangHienTai ? this.donHangHienTai.maDonHang : 0;
    const maBan = this.banDangChon ? this.banDangChon.maBan : undefined;
    
    this.nhanVienService.themMonVaoDonHienTai(maDonHang, mon.maMonAn, 1, maBan).subscribe({
      next: () => {
        if (this.banDangChon) {
          this.refreshDonHangHienTai(this.banDangChon.maBan);
          this.loadDanhSachBan();
        }
      }
    });
  }

  // NÚT THANH TOÁN: Gửi xác nhận lên hệ thống, đóng đơn hàng và làm trống bàn
  thanhToan(): void {
    if (!this.donHangHienTai) return;
    if (confirm(`Xác nhận khách hàng đã thanh toán hóa đơn cho ${this.banDangChon?.soBan}?`)) {
      this.nhanVienService.xacNhanThanhToan(this.donHangHienTai.maDonHang).subscribe({
        next: () => {
          alert('Hệ thống ghi nhận: Đã thanh toán thành công!');
          this.donHangHienTai = null;
          this.banDangChon = null;
          this.loadDanhSachBan(); // Tải lại sơ đồ bàn trống công khai
        },
        error: () => { alert('Lỗi! Không thể xác nhận thanh toán.'); }
      });
    }
  }

  // 1. Click nút "In tạm tính"
  inTamTinh(): void {
    if (!this.donHangHienTai) return;
    this.ngayInHienTai = new Date();
    this.loaiIn = 'tam-tinh';
    this.hienThiModalIn = true;
  }

  // 2. Click nút "Thanh toán (F9)" -> Hiện popup xác nhận trên màn hình
  yeuCauThanhToan(): void {
    if (!this.donHangHienTai) return;
    this.hienThiModalXacNhanThanhToan = true;
  }

  dongModalXacNhanThanhToan(): void {
    this.hienThiModalXacNhanThanhToan = false;
  }

  // 3. Người dùng bấm "Xác nhận đồng ý" trên popup thanh toán -> Gọi API Backend
  thucHienThanhToanAPI(): void {
    if (!this.donHangHienTai) return;

    this.nhanVienService.xacNhanThanhToan(this.donHangHienTai.maDonHang).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.hienThiModalXacNhanThanhToan = false; // Đóng popup xác nhận thanh toán
          this.ngayInHienTai = new Date();           // Ghi nhận thời gian hoàn tất
          this.hienThiModalHoiInSauThanhToan = true; // Mở popup hỏi muốn in hóa đơn không
        } else {
          alert(res.message);
        }
      },
      error: () => {
        alert('Có lỗi kết nối xảy ra, không thể hoàn tất thanh toán.');
      }
    });
  }

  // 4. Chọn "CÓ" - Tiến hành chuyển sang xem hóa đơn để ấn lệnh in
  dongYInHoaDonRaGiay(): void {
    this.hienThiModalHoiInSauThanhToan = false;
    this.loaiIn = 'hoa-don';
    this.hienThiModalIn = true;
  }

  // 5. Chọn "KHÔNG" - Hủy in, đóng toàn bộ và giải phóng trạng thái bàn ăn
  boQuaInHoaDon(): void {
    this.hienThiModalHoiInSauThanhToan = false;
    this.lamMoiGiaoDienSauThanhToan();
  }

  // 6. Hàm đóng cửa sổ xem trước hóa đơn
  dongModalInView(): void {
    this.hienThiModalIn = false;
    // Nếu là hóa đơn thật (sau thanh toán), đóng modal xem trước đồng nghĩa hoàn tất chu trình bàn ăn
    if (this.loaiIn === 'hoa-don') {
      this.lamMoiGiaoDienSauThanhToan();
    }
  }

  // 7. Kích hoạt lệnh in hệ thống của Trình duyệt
  kichHoatLenhIn(): void {
    window.print();
  }

  // Hàm bổ trợ dọn dẹp bộ nhớ state màn hình
  private lamMoiGiaoDienSauThanhToan(): void {
    this.donHangHienTai = null;
    this.banDangChon = null;
    this.loadDanhSachBan(); // Tải lại sơ đồ bàn để hiển thị bàn trống màu cam
  }

  moModalThemMon(): void { this.hienThiModalThemMon = true; }
  dongModalThemMon(): void { this.hienThiModalThemMon = false; }
}
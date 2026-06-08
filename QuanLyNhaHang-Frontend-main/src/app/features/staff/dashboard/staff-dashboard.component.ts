import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core'; // 1. THÊM dòng này
import { CommonModule } from '@angular/common';
import { Subscription, interval } from 'rxjs';
import { NhanVienService } from '../../../core/services/nhan-vien.service';
import { BanAnDto, DonHangNhanVienDto } from '../../../core/models/nhan-vien.model';

@Component({
  selector: 'app-staff-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './staff-dashboard.component.html',
  styleUrls: ['./staff-dashboard.component.css']
})
export class StaffDashboardComponent implements OnInit, OnDestroy {
  danhSachBan: BanAnDto[] = [];
  danhSachDonHang: DonHangNhanVienDto[] = [];
  isLoading: boolean = true;
  
  private pollingSub!: Subscription;
  private nhanVienService = inject(NhanVienService);
  private cdr = inject(ChangeDetectorRef); // 2. INJECT ChangeDetectorRef vào đây

  ngOnInit(): void {
    this.loadData();

    // Polling tự động làm mới mỗi 10 giây
    this.pollingSub = interval(10000).subscribe(() => {
      this.loadData();
    });
  }

  loadData(): void {
    console.log('--- KHỞI CHẠY LẤY DỮ LIỆU ĐỒNG BỘ ---');

    // 1. Gọi API lấy sơ đồ bàn
    this.nhanVienService.getDanhSachBan().subscribe({
      next: (res) => {
        console.log('Dữ liệu Bàn Ăn về tới Component:', res);
        if (res.isSuccess) {
          this.danhSachBan = res.data;
          this.cdr.detectChanges(); // 3. Ép Angular vẽ lại giao diện bàn ăn
        }
      },
      error: (err) => {
        console.error('Lỗi cản trở tải bàn ăn:', err);
      }
    });

    // 2. Gọi API lấy danh sách đơn hàng
    this.nhanVienService.getDonHangMoi().subscribe({
      next: (res) => {
        console.log('Dữ liệu Đơn Hàng về tới Component:', res);
        if (res.isSuccess) {
          this.danhSachDonHang = res.data;
        }
        this.isLoading = false; // Tắt trạng thái Loading
        this.cdr.detectChanges(); // 4. Ép Angular vẽ lại giao diện đơn hàng
      },
      error: (err) => {
        console.error('Lỗi cản trở tải đơn hàng:', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  ngOnDestroy(): void {
    if (this.pollingSub) {
      this.pollingSub.unsubscribe();
    }
  }
}
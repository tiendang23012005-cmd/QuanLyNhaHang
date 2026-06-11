import { Component, OnInit, inject, PLATFORM_ID, ChangeDetectorRef } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router'; // THÊM MỚI: Dùng để chuyển trang
import { CustomerService } from '../../services/customer.service';
import { AuthService } from '../../../../core/services/auth.service'; // THÊM MỚI: Hãy điều chỉnh lại số dấu chấm (../) cho đúng đường dẫn thực tế của bạn
import { MonAn, CartItem, OrderRequest } from '../../models/menu.model';

@Component({
  selector: 'app-customer-menu',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-menu.component.html',
  styleUrl: './customer-menu.component.css'
})
export class CustomerMenuComponent implements OnInit {
  menuItems: MonAn[] = [];
  cartItems: CartItem[] = [];
  totalPrice: number = 0;

  errorMessage: string = '';
  successMessage: string = '';
  isOrdering: boolean = false;

  selectedOrderType: string = 'Mang về';
  selectedPayment: string = 'Tiền mặt';
  selectedTable: number | null = null;
  orderNote: string = '';

  danhSachBan = [
    { maBan: 1, soBan: 'Bàn số 1' },
    { maBan: 2, soBan: 'Bàn số 2' },
    { maBan: 3, soBan: 'Bàn số 5' },
    { maBan: 4, soBan: 'Bàn số 6' }
  ];

  private customerService = inject(CustomerService);
  private authService = inject(AuthService); // THÊM MỚI
  private router = inject(Router);             // THÊM MỚI
  private platformId = inject(PLATFORM_ID);
  private cdr = inject(ChangeDetectorRef);

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadMenu();
    }

    this.customerService.cart$.subscribe(items => {
      this.cartItems = items;
      this.totalPrice = this.customerService.getCartTotal();
      this.cdr.detectChanges();
    });
  }

  loadMenu() {
    this.customerService.getMenu().subscribe({
      next: (data) => { this.menuItems = data; this.cdr.detectChanges(); },
      error: () => { this.errorMessage = 'Không thể tải thực đơn!'; this.cdr.detectChanges(); }
    });
  }

  onAddToCart(item: MonAn) {
    this.customerService.addToCart(item);
  }

  onRemoveFromCart(maMonAn: number) {
    this.customerService.removeFromCart(maMonAn);
  }

  async onCheckout() {
    if (this.cartItems.length === 0) return;

    // =========================================================================
    // KHU VỰC THÊM MỚI: KIỂM TRA ĐĂNG NHẬP TRƯỚC KHI ĐẶT MÓN
    // Đoạn code dưới tự động nhận diện nếu AuthService của bạn có hàm check (như isLoggedIn() hoặc isAuthenticated())
    // Nếu không tìm thấy hàm, nó sẽ tự động check sự tồn tại của Token dưới LocalStorage.
    // =========================================================================
    const hasCheckMethod = typeof this.authService.isLoggedIn === 'function';
    const isUserLoggedIn = hasCheckMethod ? this.authService.isLoggedIn() : !!localStorage.getItem('token');

    if (!isUserLoggedIn) {
      alert('Vui lòng đăng nhập vào hệ thống trước khi tiến hành đặt món!');
      this.router.navigate(['/login']);
      return; // Chặn đứng luồng xử lý tiếp theo
    }
    // =========================================================================

    if (this.selectedOrderType === 'Tại quán' && !this.selectedTable) {
      this.errorMessage = 'Vui lòng chọn số bàn!';
      return;
    }

    this.isOrdering = true;
    this.errorMessage = '';
    this.successMessage = '';

    const orderPayload: OrderRequest = {
      loaiDonHang: this.selectedOrderType,
      phuongThucThanhToan: this.selectedPayment,
      maBan: this.selectedOrderType === 'Tại quán' ? this.selectedTable : null,
      ghiChu: this.orderNote,
      chiTietDonHang: this.cartItems.map(item => ({
        maMonAn: item.monAn.maMonAn,
        soLuong: item.soLuong,
        giaLucDat: item.monAn.gia
      }))
    };

    try {
      if (this.selectedPayment === 'VNPay') {
        await this.processVNPayPayment(orderPayload);
      } else {
        const res = await this.customerService.placeOrder(orderPayload).toPromise();
        this.successMessage = 'Đặt món thành công!';
        this.customerService.clearCart();
        this.orderNote = '';
      }
    } catch (err: any) {
      this.errorMessage = err.error?.message || 'Lỗi đặt hàng. Vui lòng thử lại!';
    } finally {
      this.isOrdering = false;
      this.cdr.detectChanges();
    }
  }

  private async processVNPayPayment(orderPayload: OrderRequest) {
    try {
      const response = await this.customerService.createVNPayPayment(orderPayload).toPromise();
      
      if (response?.isSuccess && response.paymentUrl) {
        window.location.href = response.paymentUrl;
      } else {
        this.errorMessage = response?.message || 'Không thể tạo link thanh toán VNPay';
      }
    } catch (err: any) {
      this.errorMessage = 'Lỗi kết nối VNPay: ' + (err.error?.message || err.message);
    }
  }
}
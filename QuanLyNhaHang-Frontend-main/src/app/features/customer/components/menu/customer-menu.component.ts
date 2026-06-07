import { Component, OnInit, inject, PLATFORM_ID, ChangeDetectorRef } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms'; // BẮT BUỘC THÊM DÒNG NÀY ĐỂ DÙNG FORM
import { CustomerService } from '../../services/customer.service';
import { MonAn, CartItem, OrderRequest } from '../../models/menu.model';

@Component({
  selector: 'app-customer-menu',
  standalone: true,
  imports: [CommonModule, FormsModule], // THÊM FormsModule VÀO ĐÂY
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

  // --- CÁC BIẾN CHO GIAO DIỆN ĐẶT HÀNG ---
  selectedOrderType: string = 'Mang về'; 
  selectedPayment: string = 'Tiền mặt';
  selectedTable: number | null = null;
  orderNote: string = '';
  
  // Dữ liệu bàn ăn (Theo Database của cậu: MaBan 1, 2, 3, 4)
  danhSachBan = [
    { maBan: 1, soBan: 'Bàn số 1' },
    { maBan: 2, soBan: 'Bàn số 2' },
    { maBan: 3, soBan: 'Bàn số 5' },
    { maBan: 4, soBan: 'Bàn số 6' }
  ];

  private customerService = inject(CustomerService);
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
      next: (data) => {
        this.menuItems = data;
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Không thể tải thực đơn. Vui lòng kiểm tra kết nối mạng!';
        this.cdr.detectChanges(); 
      }
    });
  }

  onAddToCart(item: MonAn) {
    this.customerService.addToCart(item);
  }

  onRemoveFromCart(maMonAn: number) {
    this.customerService.removeFromCart(maMonAn);
  }

  onCheckout() {
    if (this.cartItems.length === 0) return;

    // Validate: Bắt buộc chọn bàn nếu ăn Tại quán
    if (this.selectedOrderType === 'Tại quán' && !this.selectedTable) {
      this.errorMessage = 'Vui lòng chọn số bàn nếu bạn ăn tại quán!';
      return;
    }

    this.isOrdering = true;
    this.errorMessage = '';
    this.successMessage = '';

    // Gom toàn bộ dữ liệu thực tế từ Form
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

    this.customerService.placeOrder(orderPayload).subscribe({
      next: (res) => {
        this.successMessage = 'Đặt món thành công! Đơn hàng đã được chuyển xuống bếp.';
        this.customerService.clearCart();
        this.orderNote = ''; // Xóa ghi chú sau khi đặt xong
        this.isOrdering = false;
        this.cdr.detectChanges(); 
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Lỗi đặt hàng. Vui lòng thử lại!';
        this.isOrdering = false;
        this.cdr.detectChanges(); 
      }
    });
  }
}
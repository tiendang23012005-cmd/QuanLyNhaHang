// src/app/core/services/customer.service.ts

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { MonAn, CartItem, OrderRequest } from '../models/menu.model';

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private apiUrl = 'https://localhost:7043/api'; 

  // Luồng dữ liệu quản lý giỏ hàng cục bộ (Khách chưa bấm đặt thì lưu ở đây)
  private cartSubject = new BehaviorSubject<CartItem[]>([]);
  public cart$: Observable<CartItem[]> = this.cartSubject.asObservable();

  constructor(private http: HttpClient) { }

  // 1. Lấy danh sách món ăn từ Backend
  getMenu(): Observable<MonAn[]> {
    // Lưu ý: Đổi chữ 'monan' thành đúng tên Controller Backend của bạn cậu nhé
    return this.http.get<MonAn[]>(`${this.apiUrl}/MonAn`); 
  }

  // 2. Thêm món vào giỏ hàng
  addToCart(monAn: MonAn) {
    const currentCart = this.cartSubject.value;
    const existingItem = currentCart.find(item => item.monAn.maMonAn === monAn.maMonAn);
    
    if (existingItem) {
      existingItem.soLuong++; // Nếu có rồi thì tăng số lượng
    } else {
      currentCart.push({ monAn, soLuong: 1 }); // Chưa có thì thêm mới
    }
    this.cartSubject.next([...currentCart]);
  }

  // 3. Xóa món khỏi giỏ
  removeFromCart(maMonAn: number) {
    const currentCart = this.cartSubject.value.filter(item => item.monAn.maMonAn !== maMonAn);
    this.cartSubject.next(currentCart);
  }

  // 4. Lấy tổng tiền
  getCartTotal(): number {
    return this.cartSubject.value.reduce((total, item) => total + (item.monAn.gia * item.soLuong), 0);
  }

  // 5. Gửi đơn hàng lên Backend (Truyền cho Tiến xử lý tiếp)
  placeOrder(order: OrderRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/DonHang/create`, order);
  }

  // 6. Xóa sạch giỏ hàng sau khi đặt thành công
  clearCart() {
    this.cartSubject.next([]);
  }
}
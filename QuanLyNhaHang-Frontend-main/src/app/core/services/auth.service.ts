import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { AuthResponse, UserSession } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // Cổng API chạy Backend .NET Core của bạn
  private apiUrl = 'https://localhost:7043/api/auth';

  // Luồng dữ liệu BehaviorSubject quản lý trạng thái phiên đăng nhập (Lấy từ LocalStorage nếu có)
  private currentUserSubject = new BehaviorSubject<UserSession | null>(
    typeof window !== 'undefined' ? JSON.parse(localStorage.getItem('currentUser') || 'null') : null
  );

  // Các Component khác (như Thanh điều hướng) sẽ subscribe luồng này để nhận biết User thay đổi
  public currentUser$: Observable<UserSession | null> = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  // Hàm lấy nhanh giá trị hiện tại của User (Snapshot)
  public get currentUserValue(): UserSession | null {
    return this.currentUserSubject.value;
  }

  // 1. Nghiệp vụ Đăng ký tài khoản Khách hàng
  register(user: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, user);
  }

  // 2. Nghiệp vụ Đăng nhập hệ thống (Lưu Token và phát đi trạng thái)
  login(identifier: string, matKhau: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${this.apiUrl}/login`,
      {
        Identifer: identifier,
        MatKhau: matKhau
      }
    )
    .pipe(
      tap(response => {
        if (response.isSuccess && response.token) {
          const userSession: UserSession = {
            fullName: response.fullName!,
            role: response.role!,
            token: response.token
          };
          // Lưu phiên đăng nhập vào bộ nhớ trình duyệt
          localStorage.setItem('currentUser', JSON.stringify(userSession));
          // Phát trạng thái đăng nhập mới ra toàn ứng dụng Angular
          this.currentUserSubject.next(userSession);
        }
      })
    );
  }

  // 3. Nghiệp vụ Đăng xuất
  logout() {
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null); // Phát tín hiệu NULL (Đã đăng xuất)
  }
}
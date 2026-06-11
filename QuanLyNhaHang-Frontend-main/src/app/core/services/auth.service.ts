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

  // Khởi tạo BehaviorSubject rỗng trước
  private currentUserSubject = new BehaviorSubject<UserSession | null>(null);

  // Các Component khác (như Thanh điều hướng) sẽ subscribe luồng này
  public currentUser$: Observable<UserSession | null> = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadSession(); // Tự động load session khi ứng dụng khởi chạy
  }

  // Khôi phục session từ sessionStorage (giúp F5 không bị mất đăng nhập, nhưng đóng web sẽ mất)
  private loadSession() {
    if (typeof window !== 'undefined') {
      const sessionData = sessionStorage.getItem('currentUser');
      if (sessionData) {
        try {
          this.currentUserSubject.next(JSON.parse(sessionData));
        } catch (e) {
          console.error('Lỗi parse session', e);
          sessionStorage.removeItem('currentUser');
        }
      }
    }
  }

  // Hàm lấy nhanh giá trị hiện tại của User (Snapshot)
  public get currentUserValue(): UserSession | null {
    return this.currentUserSubject.value;
  }

  // 1. Nghiệp vụ Đăng ký
  register(user: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, user);
  }

  // 2. Nghiệp vụ Đăng nhập hệ thống
  login(identifier: string, matKhau: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${this.apiUrl}/login`,
      {
        Identifer: identifier,
        MatKhau: matKhau
      }
    ).pipe(
      tap(response => {
        if (response.isSuccess && response.token) {
          const userSession: UserSession = {
            fullName: response.fullName!,
            role: response.role!,
            token: response.token
          };
          
          // Dùng sessionStorage thay vì localStorage
          sessionStorage.setItem('currentUser', JSON.stringify(userSession));
          
          // Phát trạng thái đăng nhập mới ra toàn ứng dụng
          this.currentUserSubject.next(userSession);
        }
      })
    );
  }

  // 3. Nghiệp vụ Đăng xuất
  logout() {
    sessionStorage.removeItem('currentUser');
    this.currentUserSubject.next(null); // Phát tín hiệu NULL
  }

  // SỬA LẠI HÀM NÀY:
  isLoggedIn(): boolean {
    // Lấy trạng thái từ biến currentUserValue (đã được parse chuẩn xác từ sessionStorage)
    const user = this.currentUserValue;
    
    // Nếu user có tồn tại và bên trong có chứa token -> Đã đăng nhập
    return !!(user && user.token); 
  }
}
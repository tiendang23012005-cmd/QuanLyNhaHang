import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service'; // Sửa lại đường dẫn import cho đúng cấu trúc của bạn

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const currentUser = authService.currentUserValue;

  // 1. Kiểm tra xem người dùng đã đăng nhập (có Token) chưa?
  if (currentUser && currentUser.token) {
    
    // Lấy danh sách các quyền được phép truy cập vào Trang này (được khai báo ở app.routes.ts)
    const expectedRoles = route.data['roles'] as Array<string>;

    // Nếu trang này không yêu cầu quyền cụ thể, hoặc vai trò của User nằm trong danh sách được cho phép
    if (!expectedRoles || expectedRoles.includes(currentUser.role)) {
      return true; // Hợp lệ -> Cho qua cửa!
    }

    // 2. Nếu ĐÃ ĐĂNG NHẬP nhưng SAI VAI TRÒ (Ví dụ: Khách cố vào trang Nhân viên)
    alert('Bạn không có quyền truy cập vào khu vực này!');
    
    // Bẻ hướng họ về đúng vị trí của họ
    if (currentUser.role === 'Admin') {
      router.navigate(['/admin']);
    } else if (currentUser.role === 'Nhà bếp' || currentUser.role === 'Phục vụ') {
      router.navigate(['/staff-dashboard']);
    } else {
      router.navigate(['/customer-menu']);
    }
    return false;
  }

  // 3. Nếu CHƯA ĐĂNG NHẬP -> Đá văng về trang login
  alert('Vui lòng đăng nhập để tiếp tục.');
  router.navigate(['/login']);
  return false;
};
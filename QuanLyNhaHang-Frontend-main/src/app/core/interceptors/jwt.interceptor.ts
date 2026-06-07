import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const currentUser = authService.currentUserValue;
  const isLoggedIn = currentUser && currentUser.token;

  // Nếu đã đăng nhập, tiến hành nhân bản request và đính kèm Token theo chuẩn Bearer
  if (isLoggedIn) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${currentUser.token}`
      }
    });
  }

  return next(req);
};
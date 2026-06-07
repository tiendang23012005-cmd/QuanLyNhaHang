import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http'; // Sửa lại thành withInterceptors
import { jwtInterceptor } from './core/interceptors/jwt.interceptor'; // Import interceptor vừa tạo

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([jwtInterceptor])) // Đăng ký thành công Interceptor chạy ngầm
  ]
};

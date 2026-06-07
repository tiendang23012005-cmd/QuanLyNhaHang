import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register'; // Nhớ check đúng tên file register.ts của bạn
import { CustomerMenuComponent } from './features/customer/components/menu/customer-menu.component';
export const routes: Routes = [
  { 
    path: 'menu', 
    component: CustomerMenuComponent 
  },
  { 
    path: '', 
    redirectTo: 'menu', 
    pathMatch: 'full' 
  },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent }, // THÊM DÒNG NÀY: Khai báo định tuyến trang đăng ký
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', redirectTo: '/login' }
];
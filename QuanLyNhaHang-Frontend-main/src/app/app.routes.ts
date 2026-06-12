import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register'; 
import { CustomerMenuComponent } from './features/customer/components/menu/customer-menu.component';
import { StaffDashboardComponent } from './features/staff/dashboard/staff-dashboard.component';
import { UserManagement } from './features/auth/admin/user-management/user-management';
import { PaymentFailedComponent } from './features/customer/pages/payment-failed/payment-failed';
import { PaymentSuccessComponent } from './features/customer/pages/payment-success/payment-success';
import { AdminLayout } from './layouts/admin-layout/admin-layout';

// THÊM DÒNG NÀY: Import authGuard mà bạn đã tạo ở Phần 2
import { authGuard } from './core/guards/auth.guard'; // Điều chỉnh lại đường dẫn cho đúng vị trí file của bạn

export const routes: Routes = [
  // 1. CÁC ROUTE CỤ THỂ (Khai báo trước)
  { 
    path: 'login', 
    component: LoginComponent 
  },
  { 
    path: 'register', 
    component: RegisterComponent 
  },
  { 
    path: 'menu', 
    component: CustomerMenuComponent 
    // Nếu bạn muốn KHÁCH HÀNG cũng phải đăng nhập mới được xem menu, hãy mở comment 2 dòng dưới:
    // canActivate: [authGuard],
    // data: { roles: ['Khách hàng', 'Admin'] }
  },
  { 
    path: 'staff-dashboard', 
    component: StaffDashboardComponent,
    // BẢO VỆ ROUTE NÀY: Chỉ những ai đăng nhập với các role dưới đây mới được vào
    canActivate: [authGuard],
    data: { roles: ['Phục vụ', 'Nhà bếp'] }
  },

  // ĐƯA 2 ROUTE THANH TOÁN LÊN TRÊN NÀY ĐỂ ANGULAR ĐỌC ĐƯỢC
  { 
    path: 'payment-success', 
    component: PaymentSuccessComponent 
  },
  { 
    path: 'payment-failed', 
    component: PaymentFailedComponent 
  },

  {
    path: 'admin',
    component: AdminLayout,
    children: [

      {
        path: '',
        redirectTo: 'user-management',
        pathMatch: 'full'
      },

      {
        path: 'user-management',
        loadComponent: () =>
          import('./features/auth/admin/user-management/user-management')
            .then(m => m.UserManagement)
      },

      {
        path: 'create-employee',
        loadComponent: () =>
          import('./features/auth/admin/user-management/create/create')
            .then(m => m.Create)
      },

      {
        path: 'edit-employee/:id',
        loadComponent: () =>
          import('./features/auth/admin/user-management/edit-employee/edit-employee')
            .then(m => m.EditEmployee)
      },

      {
        path: 'table-management',
        loadComponent: () =>
          import('./features/auth/admin/user-management/table-management/table-management')
            .then(m => m.TableManagement)
      },

      {
        path: 'create-table',
        loadComponent: () =>
          import('./features/auth/admin/user-management/table-management/create-table/create-table')
            .then(m => m.CreateTable)
      },

      {
        path: 'edit-table/:id',
        loadComponent: () =>
          import('./features/auth/admin/user-management/table-management/edit-table/edit-table')
            .then(m => m.EditTable)
      },

      {
        path: 'food-management',
        loadComponent: () =>
          import('./features/auth/admin/user-management/food-management/food-management')
            .then(m => m.FoodManagement)
      },

      {
        path: 'create-food',
        loadComponent: () =>
          import('./features/auth/admin/user-management/food-management/create-food/create-food')
            .then(m => m.CreateFood)
      },

      {
        path: 'edit-food/:id',
        loadComponent: () =>
          import('./features/auth/admin/user-management/food-management/edit-food/edit-food')
            .then(m => m.EditFood)
      },

      {
        path: 'revenue',
        loadComponent: () =>
          import('./features/auth/admin/user-management/revenue/revenue')
            .then(m => m.Revenue)
      }

    ]
  },

  // 2. ROUTE MẶC ĐỊNH (Khi người dùng gõ localhost:4200)
  // Nếu bạn muốn vừa vào trang web là bắt Đăng nhập ngay, hãy sửa 'menu' thành 'login'
  { 
    path: '', 
    redirectTo: 'menu', 
    pathMatch: 'full' 
  },

  // 3. ROUTE CATCH-ALL (Wildcard - Xử lý đường dẫn bậy bạ)
  { 
    path: '**', 
    redirectTo: 'menu' 
  }
];
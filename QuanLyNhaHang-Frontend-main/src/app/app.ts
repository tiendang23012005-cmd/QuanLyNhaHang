import { Component, signal, inject } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms'; 
import { HeaderComponent } from './features/shared/header/header.component';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true, 
  imports: [
    RouterOutlet,
    HeaderComponent,
    ReactiveFormsModule 
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('QuanLyNhaHangWeb');
  
  // Biến Signal quản lý việc ẩn/hiện Header toàn cục
  protected readonly showHeader = signal(true);

  private router = inject(Router);

  constructor() {
    // Theo dõi mọi sự kiện chuyển trang thành công trong hệ thống
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      const currentUrl = event.urlAfterRedirects || event.url;
      
      // Kiểm tra xem người dùng có đang ở trang login hoặc register không
      const isAuthPage = currentUrl.includes('/login') || currentUrl.includes('/register');
      
      // Cập nhật trạng thái hiển thị Header (Ẩn nếu ở trang Auth, Hiện nếu ở trang khác)
      this.showHeader.set(!isAuthPage);
    });
  }
}
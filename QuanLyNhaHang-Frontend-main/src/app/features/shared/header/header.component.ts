import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserSession } from '../../../core/models/auth.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css' 
})
export class HeaderComponent implements OnInit, OnDestroy {
  isLoggedIn = false;
  fullName = 'Khách';
  
  // Biến lưu trữ subscription để dọn dẹp khi cần
  private userSubscription!: Subscription;

  // Sử dụng inject (tương tự như trong login.ts của bạn)
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit(): void {
    // Lắng nghe liên tục sự thay đổi trạng thái User
    this.userSubscription = this.authService.currentUser$.subscribe({
      next: (user: UserSession | null) => {
        if (user) {
          this.isLoggedIn = true;
          this.fullName = user.fullName || 'Khách';
        } else {
          this.isLoggedIn = false;
          this.fullName = 'Khách';
        }
      }
    });
  }

  logout() {
    // Chỉ cần gọi hàm logout của Service, luồng currentUser$ sẽ tự động nhận Null
    // và UI sẽ tự động cập nhật về trạng thái 'Khách'
    this.authService.logout();
    this.router.navigate(['/login'], { replaceUrl: true });
  }

  ngOnDestroy(): void {
    // Hủy lắng nghe khi component bị hủy để tránh rò rỉ bộ nhớ (Memory Leak)
    if (this.userSubscription) {
      this.userSubscription.unsubscribe();
    }
  }
}
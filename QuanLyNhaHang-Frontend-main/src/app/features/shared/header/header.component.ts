import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './header.component.html',
  styleUrl:'./header.component.css' 
})
export class HeaderComponent implements OnInit {   // ← Phải có 'export class'
  
  isLoggedIn = false;
  fullName = 'Khách';

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.loadUserInfo();
  }

  loadUserInfo() {
    const userJson = localStorage.getItem('currentUser');
    if (userJson) {
      try {
        const user = JSON.parse(userJson);
        this.fullName = user.fullName || user.hoTen || user.tenDangNhap || 'Khách';
        this.isLoggedIn = true;
      } catch (e) {
        console.error('Lỗi parse user info', e);
      }
    }
  }

  logout() {
    localStorage.removeItem('currentUser');
    this.isLoggedIn = false;
    this.fullName = 'Khách';
    this.router.navigate(['/login'], { replaceUrl: true });
  }
}
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { AuthResponse } from '../../../core/models/auth.model';
import { finalize } from 'rxjs/operators';

declare var $: any;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  errorMessage: string = '';
  isLoading: boolean = false;

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit(): void {
    // Tên các thuộc tính trong group viết thường để đồng bộ với HTML
    this.loginForm = this.fb.group({
      identifer: ['', [Validators.required]],
      matKhau: ['', [Validators.required, Validators.minLength(6)]]
    });

    $(document).ready(function() {
      $('.auth-card').hide().fadeIn(800);
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';

    // Ép kiểu chuẩn xác 100% sang đối tượng LoginRequest của Backend .NET
    const loginPayload = {
      Identifer: this.loginForm.value.identifer,
      MatKhau: this.loginForm.value.matKhau
    };

    // Truyền loginPayload vào hàm đăng nhập của AuthService
    this.authService.login(loginPayload.Identifer, loginPayload.MatKhau)
      .pipe(
        finalize(() => {
          this.isLoading = false; // Luôn luôn tắt loading dứt điểm
        })
      )
      .subscribe({
        next: (res: AuthResponse) => {
          console.log('Response từ API:', res);

          if (res.isSuccess) {
            alert('Đăng nhập thành công');

            if (res.role === 'Admin') {
              this.router.navigate(['/admin']);
            } else if (res.role === 'Nhà bếp' || res.role === 'Phục vụ') {
              this.router.navigate(['/staff']);
            } else {
              this.router.navigate(['/customer']);
            }
          } else {
            this.errorMessage = res.message;
          }
        }
      });
  }
}
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { AuthResponse } from '../../../core/models/auth.model';
import { finalize } from 'rxjs/operators';

declare var $: any;

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  errorMessage: string = '';
  successMessage: string = '';
  isLoading: boolean = false;

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      hoTen: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      dienThoai: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]], // dienThoai viết thường đồng bộ hoàn toàn
      matKhau: ['', [Validators.required, Validators.minLength(6)]],
      nhapLaiMatKhau: ['', [Validators.required]]
    }, {
      validators: this.passwordMatchValidator
    });

    $(document).ready(function() {
      $('.register-card').hide().fadeIn(800);
    });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('matKhau')?.value === g.get('nhapLaiMatKhau')?.value
      ? null : { mismatch: true };
  }

  onSubmit(): void {
    if (this.registerForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const formValues = this.registerForm.value;

    // Ép kiểu chuyển dữ liệu khớp chính xác 100% với RegisterRequest bên phía C# .NET
    const registerPayload = {
      HoTen: formValues.hoTen,
      Email: formValues.email,
      DienThoai: formValues.dienThoai,
      MatKhau: formValues.matKhau
    };

    this.authService.register(registerPayload)
      .pipe(
        finalize(() => {
          this.isLoading = false;
        })
      )
      .subscribe({
        next: (res: AuthResponse) => {
          if (res.isSuccess) {
            this.successMessage = res.message || 'Đăng ký tài khoản thành công!';
            this.registerForm.reset();
          }
        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Đăng ký thất bại. Dữ liệu không hợp lệ!';
        }
      });
}
}
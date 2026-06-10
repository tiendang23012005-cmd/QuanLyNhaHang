import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css',
})
export class UserManagement implements OnInit {
  // Lấy thực thể form từ HTML để dùng cho các Getters ở dưới
  @ViewChild('employeeForm') employeeForm!: NgForm;

  Users: any[] = [];
  isLoading: boolean = false; // Trạng thái đợi khi đang gọi API

  // Đối tượng chứa dữ liệu nhập từ form
  NewUser = {
    hoTen: '',
    email: '',
    dienThoai: '',
    matKhau: '',
    maVaiTro: 2
  };

  constructor(private Http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.LoadUsers();
  }

  // Lấy toàn bộ danh sách nhân viên về
  LoadUsers() {
    this.Http.get<any[]>(
      'https://localhost:7043/api/UserManagement'
    ).subscribe({
      next: (data) => {
        this.Users = data;
        this.cdr.detectChanges(); // Ép hiển thị dữ liệu ra màn hình ngay lập tức
      },
      error: (err) => {
        console.error('Lỗi khi tải danh sách người dùng:', err);
      }
    });
  }

  // Hàm xử lý khi bấm nút Thêm nhân viên
  AddEmployee(form: NgForm) {
    if (form.valid) {
      this.isLoading = true;

      this.Http.post(
        'https://localhost:7043/api/UserManagement',
        this.NewUser
      ).subscribe({
        next: () => {
          alert('Thêm nhân viên thành công!');

          // Reset sạch sẽ trạng thái lỗi đỏ của form trên UI
          form.resetForm({
            maVaiTro: 2 // Đặt lại vai trò mặc định ban đầu là Phục vụ
          });

          // Đưa object NewUser về lại trạng thái trống
          this.NewUser = {
            hoTen: '',
            email: '',
            dienThoai: '',
            matKhau: '',
            maVaiTro: 2
          };

          this.isLoading = false;
          this.LoadUsers(); // Tải lại danh sách nhân viên mới cập nhật
        },
        error: (err) => {
          console.error('Lỗi khi gọi API thêm nhân viên:', err);
          this.isLoading = false;
          alert('Không thể thêm nhân viên: ' + (err.error?.message || 'Lỗi kết nối đến Server.'));
        }
      });
    } else {
      // Nếu cố tình submit khi form không hợp lệ, đánh dấu tất cả đã chạm để hiện lỗi đỏ
      form.control.markAllAsTouched();
      console.error('Form nhân viên không hợp lệ');
    }
  }

  // Thay đổi vai trò nhân viên trực tiếp trên thẻ select của Table
  ChangeRole(user: any) {
    this.Http.put(
      `https://localhost:7043/api/UserManagement/${user.maNguoiDung}/role`,
      {
        maVaiTro: user.maVaiTro
      }
    ).subscribe({
      next: () => {
        alert('Cập nhật vai trò nhân viên thành công');
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
        alert('Cập nhật vai trò thất bại');
      }
    });
  }

  // Khóa hoặc mở khóa tài khoản nhân viên
  ToggleStatus(user: any) {
    const trangThaiMoi = !user.trangThaiHoatDong;

    this.Http.put(
      `https://localhost:7043/api/UserManagement/${user.maNguoiDung}/status`,
      {
        trangThaiHoatDong: trangThaiMoi
      }
    ).subscribe({
      next: () => {
        user.trangThaiHoatDong = trangThaiMoi; // Cập nhật lại biến local sau khi API chạy xong
        this.cdr.detectChanges(); // Re-render lại giao diện nút bấm tức thì
      },
      error: (err) => {
        console.error(err);
        alert('Không thể thay đổi trạng thái tài khoản');
      }
    });
  }

  // ==================== GETTERS ĐỂ ĐỒNG BỘ CẤU TRÚC BÁO LỖI VỚI HTML ====================
  get hoTen() {
    return this.employeeForm?.controls['hoTen'];
  }

  get email() {
    return this.employeeForm?.controls['email'];
  }

  get dienThoai() {
    return this.employeeForm?.controls['dienThoai'];
  }

  get matKhau() {
    return this.employeeForm?.controls['matKhau'];
  }

  get maVaiTro() {
    return this.employeeForm?.controls['maVaiTro'];
  }
}
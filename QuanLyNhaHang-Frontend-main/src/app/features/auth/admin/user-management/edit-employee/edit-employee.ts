import {
  Component,
  OnInit
} from '@angular/core';

import { CommonModule } from '@angular/common';

import { FormsModule } from '@angular/forms';

import { HttpClient } from '@angular/common/http';

import {
  ActivatedRoute,
  Router
} from '@angular/router';

@Component({
  selector: 'app-edit-employee',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './edit-employee.html',
  styleUrl: './edit-employee.css'
})
export class EditEmployee implements OnInit {

  UserId = 0;

  User = {
    hoTen: '',
    email: '',
    dienThoai: '',
    maVaiTro: 2
  };

  constructor(
    private http: HttpClient,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {

    this.UserId = Number(
      this.route.snapshot.paramMap.get('id')
    );

    this.LoadUser();

  }

  LoadUser() {

    this.http.get<any>(
      `https://localhost:7043/api/UserManagement/${this.UserId}`
    )
    .subscribe({

      next: (data) => {

        this.User = data;

      },

      error: () => {

        alert('Không tìm thấy nhân viên');

      }

    });

  }

  UpdateEmployee() {

    if (
      !this.User.hoTen.trim() ||
      !this.User.email.trim() ||
      !this.User.dienThoai.trim()
    ) {

      alert(
        'Vui lòng nhập đầy đủ thông tin'
      );

      return;

    }

    this.http.put(
      `https://localhost:7043/api/UserManagement/${this.UserId}`,
      this.User
    )
    .subscribe({

      next: () => {

        alert('Cập nhật thành công');

        this.router.navigateByUrl(
          '/admin/user-management'
        );

      },

      error: () => {

        alert('Cập nhật thất bại');

      }

    });

  }

}
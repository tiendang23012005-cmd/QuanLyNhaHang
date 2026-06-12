import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-create',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './create.html',
  styleUrl: './create.css'
})
export class Create {

  NewUser = {
    hoTen: '',
    email: '',
    dienThoai: '',
    matKhau: '',
    maVaiTro: 2
  };

  constructor(
    private http: HttpClient,
    private router: Router
  ) {}

  AddEmployee() {

    this.http.post(
      'https://localhost:7043/api/UserManagement',
      this.NewUser
    )
    .subscribe({

      next: () => {

        alert('Thêm nhân viên thành công');

        this.router.navigate([
          '/admin/user-management'
        ]);

      },

      error: (err) => {

        console.log(err);

        alert('Thêm nhân viên thất bại');

      }

    });

  }

}

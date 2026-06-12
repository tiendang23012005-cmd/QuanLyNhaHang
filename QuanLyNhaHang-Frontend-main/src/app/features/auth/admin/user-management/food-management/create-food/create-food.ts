import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-create-food',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './create-food.html',
  styleUrl: './create-food.css'
})
export class CreateFood {

  constructor(
    private http: HttpClient,
    private router: Router
  ) { }

  Food = {
    maDanhMuc: 1,
    tenMonAn: '',
    gia: 0,
    moTa: '',
    hinhAnh: '',
    conBan: true
  };

  Save() {

    if (
      !this.Food.tenMonAn.trim() ||
      this.Food.gia <= 0
    ) {

      alert('Vui lòng nhập đầy đủ thông tin');

      return;

    }

    this.http.post(
      'https://localhost:7043/api/FoodManagement',
      this.Food
    )
    .subscribe({

      next: () => {

        alert('Thêm món thành công');

        this.router.navigate([
          '/admin/food-management'
        ]);

      },

      error: (error) => {

        console.error(error);

        alert('Thêm món thất bại');

      }

    });

  }

  Cancel() {

    this.router.navigate([
      '/admin/food-management'
    ]);

  }

}
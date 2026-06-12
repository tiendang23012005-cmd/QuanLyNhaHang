import {
  Component,
  OnInit,
  ChangeDetectorRef
} from '@angular/core';

import { CommonModule } from '@angular/common';

import { FormsModule } from '@angular/forms';

import { Router } from '@angular/router';

import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-food-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './food-management.html',
  styleUrl: './food-management.css'
})
export class FoodManagement implements OnInit {

  SearchText = '';

  Foods: any[] = [];

  constructor(
    private router: Router,
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {

    this.LoadFoods();

  }

  LoadFoods() {

    this.http.get<any[]>(
      'https://localhost:7043/api/FoodManagement'
    )
    .subscribe({

      next: (data) => {

        this.Foods = data.map(x => ({

          maMon: x.maMonAn,

          tenMon: x.tenMonAn,

          loaiMon: x.danhMuc,

          gia: x.gia,

          hinhAnh:
            x.hinhAnh ||

            (x.danhMuc === 'Món chính'
              ? '🍛'
              : x.danhMuc === 'Nước uống'
              ? '🥤'
              : x.danhMuc === 'Tráng miệng'
              ? '🍰'
              : x.danhMuc === 'Thức ăn nhanh'
              ? '🍟'
              : '🍽'),

          trangThai: x.conBan

        }));

        this.cdr.detectChanges();

      },

      error: (err) => {

        console.error(err);

        alert(
          'Không tải được danh sách món ăn'
        );

      }

    });

  }

  get FilteredFoods() {

    return this.Foods.filter(food =>
      food.tenMon
        .toLowerCase()
        .includes(
          this.SearchText.toLowerCase()
        )
    );

  }

  AddFood() {

    this.router.navigate([
      '/admin/create-food'
    ]);

  }

  EditFood(food: any) {

    this.router.navigate([
      '/admin/edit-food',
      food.maMon
    ]);

  }

  DeleteFood(food: any) {

    const confirmDelete = confirm(
      `Bạn có chắc muốn xóa ${food.tenMon}?`
    );

    if (!confirmDelete) {

      return;

    }

    this.http.delete(
      `https://localhost:7043/api/FoodManagement/${food.maMon}`
    )
    .subscribe({

      next: () => {

        alert('Xóa thành công');

        this.LoadFoods();

      },

      error: (err) => {

        console.error(err);

        alert('Xóa thất bại');

      }

    });

  }

}
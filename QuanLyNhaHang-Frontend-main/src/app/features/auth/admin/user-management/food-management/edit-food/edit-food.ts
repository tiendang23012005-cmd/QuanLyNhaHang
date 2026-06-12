import {
  Component,
  OnInit
} from '@angular/core';

import { CommonModule } from '@angular/common';

import { FormsModule } from '@angular/forms';

import { HttpClient } from '@angular/common/http';

import {
  ActivatedRoute,
  Router,
  RouterModule
} from '@angular/router';

@Component({
  selector: 'app-edit-food',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './edit-food.html',
  styleUrl: './edit-food.css',
})
export class EditFood implements OnInit {

  FoodId = 0;

  Food = {
    maDanhMuc: 1,
    tenMonAn: '',
    gia: 0,
    moTa: '',
    hinhAnh: '',
    conBan: true
  };

  IsLoading = false;

  constructor(
    private http: HttpClient,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {

    this.FoodId = Number(
      this.route.snapshot.paramMap.get('id')
    );

    this.LoadFood();

  }

  LoadFood(): void {

    this.http.get<any[]>(
      'https://localhost:7043/api/FoodManagement'
    )
    .subscribe({

      next: (data) => {

        const food = data.find(
          x => x.maMonAn === this.FoodId
        );

        if (!food) {

          alert('Không tìm thấy món ăn');

          this.GoBack();

          return;

        }

        this.Food = {

          maDanhMuc: food.maDanhMuc,

          tenMonAn: food.tenMonAn,

          gia: food.gia,

          moTa: food.moTa || '',

          hinhAnh: food.hinhAnh || '',

          conBan: food.conBan

        };

      },

      error: () => {

        alert('Không tải được dữ liệu');

      }

    });

  }

  Save(): void {

    if (
      !this.Food.tenMonAn.trim() ||
      this.Food.gia <= 0
    ) {

      alert('Vui lòng nhập đầy đủ thông tin');

      return;

    }

    this.IsLoading = true;

    this.http.put(
      `https://localhost:7043/api/FoodManagement/${this.FoodId}`,
      this.Food
    )
    .subscribe({

      next: () => {

        alert('Cập nhật món ăn thành công');

        this.IsLoading = false;

        this.router.navigateByUrl(
          '/admin/food-management'
        );

      },

      error: (err) => {

        console.log(err);

        this.IsLoading = false;

        alert('Không thể cập nhật món ăn');

      }

    });

  }

  GoBack(): void {

    this.router.navigateByUrl(
      '/admin/food-management'
    );

  }

}
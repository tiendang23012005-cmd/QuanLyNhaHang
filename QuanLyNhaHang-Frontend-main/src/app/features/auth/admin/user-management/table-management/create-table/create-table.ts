import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-create-table',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './create-table.html',
  styleUrl: './create-table.css'
})
export class CreateTable {

  NewTable = {
    soBan: '',
    sucChua: 4,
    trangThaiBan: 'Trống'
  };

  IsLoading = false;

  constructor(
    private http: HttpClient,
    private router: Router
  ) {}

  AddTable(): void {

    if (
      !this.NewTable.soBan.trim() ||
      this.NewTable.sucChua <= 0
    ) {

      alert('Vui lòng nhập đầy đủ thông tin');

      return;

    }

    this.IsLoading = true;

    this.http.post(
      'https://localhost:7043/api/TableManagement',
      this.NewTable
    )
    .subscribe({

      next: () => {

        alert('Thêm bàn thành công');

        this.IsLoading = false;

        this.router.navigateByUrl(
          '/admin/table-management'
        );

      },

      error: (err) => {

        console.log(err);

        this.IsLoading = false;

        alert(
          err?.error ||
          'Không thể thêm bàn'
        );

      }

    });

  }

  GoBack(): void {

    this.router.navigateByUrl(
      '/admin/table-management'
    );

  }

}
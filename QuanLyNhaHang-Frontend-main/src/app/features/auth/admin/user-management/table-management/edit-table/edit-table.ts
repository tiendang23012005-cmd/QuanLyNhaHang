import { Component, OnInit } from '@angular/core';

import { CommonModule } from '@angular/common';

import { FormsModule } from '@angular/forms';

import { HttpClient } from '@angular/common/http';

import {
  ActivatedRoute,
  Router,
  RouterModule
} from '@angular/router';

@Component({
  selector: 'app-edit-table',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './edit-table.html',
  styleUrl: './edit-table.css',
})
export class EditTable implements OnInit {

  TableId = 0;

  Table = {
    soBan: '',
    sucChua: 4,
    trangThaiBan: 'Trống'
  };

  IsLoading = false;

  constructor(
    private http: HttpClient,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {

    this.TableId = Number(
      this.route.snapshot.paramMap.get('id')
    );

    this.LoadTable();

  }

  LoadTable(): void {

    this.http.get<any[]>(
      'https://localhost:7043/api/TableManagement'
    )
    .subscribe({

      next: (data) => {

        const table = data.find(
          x => x.maBan === this.TableId
        );

        if (!table) {

          alert('Không tìm thấy bàn');

          this.GoBack();

          return;

        }

        this.Table = {

          soBan: table.soBan,

          sucChua: table.sucChua,

          trangThaiBan: table.trangThaiBan

        };

      },

      error: () => {

        alert('Không tải được dữ liệu');

      }

    });

  }

  Save(): void {

    if (
      !this.Table.soBan.trim() ||
      this.Table.sucChua <= 0
    ) {

      alert('Vui lòng nhập đầy đủ thông tin');

      return;

    }

    this.IsLoading = true;

    this.http.put(
      `https://localhost:7043/api/TableManagement/${this.TableId}`,
      this.Table
    )
    .subscribe({

      next: () => {

        alert('Cập nhật thành công');

        this.IsLoading = false;

        this.router.navigateByUrl(
          '/admin/table-management'
        );

      },

      error: (err) => {

        console.log(err);

        this.IsLoading = false;

        alert('Không thể cập nhật bàn');

      }

    });

  }

  GoBack(): void {

    this.router.navigateByUrl(
      '/admin/table-management'
    );

  }

}
import {
  Component,
  OnInit,
  ChangeDetectorRef
} from '@angular/core';

import { CommonModule } from '@angular/common';

import { HttpClient } from '@angular/common/http';

import {
  Router,
  RouterModule
} from '@angular/router';

@Component({
  selector: 'app-table-management',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule
  ],
  templateUrl: './table-management.html',
  styleUrl: './table-management.css',
})
export class TableManagement implements OnInit {

  Tables: any[] = [];

  constructor(
    private http: HttpClient,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {

    this.LoadTables();

  }

  LoadTables(): void {

    this.http.get<any[]>(
      'https://localhost:7043/api/TableManagement'
    )
    .subscribe({

      next: (data) => {

        this.Tables = [...data];

        this.cdr.detectChanges();

      },

      error: (err) => {

        console.log(err);

        alert('Không tải được danh sách bàn');

      }

    });

  }

  AddTable(): void {

    this.router.navigateByUrl(
      '/admin/create-table'
    );

  }

  EditTable(table: any): void {

    this.router.navigate([
      '/admin/edit-table',
      table.maBan
    ]);

  }

  DeleteTable(table: any): void {

    const confirmDelete = confirm(
      `Bạn có chắc muốn xóa ${table.soBan}?`
    );

    if (!confirmDelete) {

      return;

    }

    this.http.delete(
      `https://localhost:7043/api/TableManagement/${table.maBan}`
    )
    .subscribe({

      next: () => {

        alert('Xóa thành công');

        this.LoadTables();

      },

      error: (err) => {

        console.log(err);

        alert('Không thể xóa bàn');

      }

    });

  }

}
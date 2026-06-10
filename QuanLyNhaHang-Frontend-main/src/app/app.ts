import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms'; // Thêm dòng này để chuẩn bị làm Form Đăng nhập/Đăng ký
import { HeaderComponent } from './features/shared/header/header.component';

@Component({
  selector: 'app-root',
  standalone: true, // Bản chất các bản mới đã tự bật chế độ này
  imports: [RouterOutlet,
    HeaderComponent,
    ReactiveFormsModule // Khai báo ReactiveFormsModule tại đây để kích hoạt [formGroup]
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('QuanLyNhaHangWeb');
}

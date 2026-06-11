import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-payment-success',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl:`./payment-success.html`,
  styleUrl: `./payment-success.css`,
})
export class PaymentSuccessComponent implements OnInit {
  orderId: string | null = '';
  private route = inject(ActivatedRoute);

  ngOnInit(): void {
    // Lấy OrderId từ thanh địa chỉ (URL)
    this.route.queryParams.subscribe(params => {
      this.orderId = params['OrderId'];
    });
  }
}
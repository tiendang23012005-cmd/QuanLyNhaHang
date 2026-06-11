import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-payment-failed',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl:`./payment-failed.html`,
  styleUrl: `./payment-failed.css`,
})
export class PaymentFailedComponent implements OnInit {
  rawOrderId: string = '';
  orderIdDisplay: string = ''; 
  errorMessage: string | null = '';
  
  private route = inject(ActivatedRoute);

  ngOnInit(): void {
    // Lấy OrderId và message từ thanh địa chỉ (URL)
    this.route.queryParams.subscribe(params => {
      this.rawOrderId = params['OrderId'] || '';
      this.errorMessage = params['message'] || '';
      
      // Cắt bỏ chữ _ANG trước khi hiển thị (giống trang Success)
      this.orderIdDisplay = this.rawOrderId.replace('_ANG', ''); 
    });
  }
}
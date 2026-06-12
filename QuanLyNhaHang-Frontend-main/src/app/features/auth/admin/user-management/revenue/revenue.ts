import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-revenue',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './revenue.html',
  styleUrl: './revenue.css'
})
export class Revenue {

  TotalRevenue = 0;

  TotalBills = 0;

  TotalCustomers = 0;

  MonthlyTarget = 100000000;

  MonthlyRevenue = [
    { month: 'T1', revenue: 0 },
    { month: 'T2', revenue: 0 },
    { month: 'T3', revenue: 0 },
    { month: 'T4', revenue: 0 },
    { month: 'T5', revenue: 0 },
    { month: 'T6', revenue: 0 },
    { month: 'T7', revenue: 0 },
    { month: 'T8', revenue: 0 },
    { month: 'T9', revenue: 0 },
    { month: 'T10', revenue: 0 },
    { month: 'T11', revenue: 0 },
    { month: 'T12', revenue: 0 }
  ];

  RevenueDetails = [
    { month: 'Tháng 1', revenue: 0, bills: 0, customers: 0 },
    { month: 'Tháng 2', revenue: 0, bills: 0, customers: 0 },
    { month: 'Tháng 3', revenue: 0, bills: 0, customers: 0 },
    { month: 'Tháng 4', revenue: 0, bills: 0, customers: 0 },
    { month: 'Tháng 5', revenue: 0, bills: 0, customers: 0 },
    { month: 'Tháng 6', revenue: 0, bills: 0, customers: 0 },
    { month: 'Tháng 7', revenue: 0, bills: 0, customers: 0 },
    { month: 'Tháng 8', revenue: 0, bills: 0, customers: 0 },
    { month: 'Tháng 9', revenue: 0, bills: 0, customers: 0 },
    { month: 'Tháng 10', revenue: 0, bills: 0, customers: 0 },
    { month: 'Tháng 11', revenue: 0, bills: 0, customers: 0 },
    { month: 'Tháng 12', revenue: 0, bills: 0, customers: 0 }
  ];

  get Progress(): number {

    if (this.MonthlyTarget === 0) {
      return 0;
    }

    return Math.round(
      (this.TotalRevenue / this.MonthlyTarget) * 100
    );

  }

  GetBarHeight(revenue: number): string {

    return `${Math.max(revenue, 5)}px`;

  }

}

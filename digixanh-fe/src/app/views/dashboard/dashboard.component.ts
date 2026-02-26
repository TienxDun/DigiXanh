import { ChangeDetectionStrategy, ChangeDetectorRef, Component } from '@angular/core';
import { ChartData, ChartOptions } from 'chart.js';
import { ChartjsComponent } from '@coreui/angular-chartjs';
import { RouterLink } from '@angular/router';
import { NgIf } from '@angular/common';
import { finalize } from 'rxjs/operators';
import { AdminDashboardDto } from '../../core/models/dashboard.model';
import { AdminDashboardService } from '../../core/services/admin-dashboard.service';

@Component({
  templateUrl: 'dashboard.component.html',
  styleUrls: ['dashboard.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    RouterLink,
    ChartjsComponent,
    NgIf
  ]
})
export class DashboardComponent {
  private readonly primaryColor = getComputedStyle(document.documentElement)
    .getPropertyValue('--cui-primary')
    .trim() || '#5856d6';

  dashboard: AdminDashboardDto | null = null;
  loading = false;
  errorMessage = '';

  chartData: ChartData<'bar'> = {
    labels: [],
    datasets: [
      {
        label: 'Đơn hàng',
        data: [],
        backgroundColor: this.primaryColor
      }
    ]
  };

  chartOptions: ChartOptions<'bar'> = {
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          precision: 0
        }
      }
    }
  };

  constructor(
    private dashboardService: AdminDashboardService,
    private cdr: ChangeDetectorRef
  ) {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;
    this.errorMessage = '';

    this.dashboardService
      .getAdminDashboard()
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (dashboard) => {
          this.dashboard = dashboard;
          this.chartData = {
            labels: dashboard.dailyOrders.map(item => item.label),
            datasets: [
              {
                label: 'Đơn hàng',
                data: dashboard.dailyOrders.map(item => item.ordersCount),
                backgroundColor: this.primaryColor
              }
            ]
          };
          this.cdr.markForCheck();
        },
        error: () => {
          this.errorMessage = 'Không thể tải dữ liệu dashboard. Vui lòng thử lại.';
          this.cdr.markForCheck();
        }
      });
  }

  formatCurrency(value: number | undefined): string {
    if (value === undefined) {
      return '0 ₫';
    }

    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      maximumFractionDigits: 0
    }).format(value);
  }
}

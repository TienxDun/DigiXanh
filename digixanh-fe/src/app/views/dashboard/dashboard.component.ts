import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { Chart, ChartOptions } from 'chart.js/auto';
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
    NgIf
  ]
})
export class DashboardComponent implements AfterViewInit, OnDestroy {
  @ViewChild('ordersChart')
  set ordersChartRef(value: ElementRef<HTMLCanvasElement> | undefined) {
    this._ordersChartRef = value;
    this.renderChart();
  }

  private _ordersChartRef?: ElementRef<HTMLCanvasElement>;
  private readonly rootStyles = getComputedStyle(document.documentElement);
  private readonly primaryColor = this.rootStyles.getPropertyValue('--cui-primary').trim() || '#2e7d32';
  private readonly bodyColor = this.rootStyles.getPropertyValue('--cui-body-color').trim() || '#343a40';
  private readonly borderColor = this.rootStyles.getPropertyValue('--cui-border-color').trim() || '#dee2e6';
  private chartInstance: Chart<'bar'> | null = null;
  private chartLabels: string[] = [];
  private chartValues: number[] = [];

  dashboard: AdminDashboardDto | null = null;
  loading = false;
  errorMessage = '';

  chartOptions: ChartOptions<'bar'> = {
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false
      }
    },
    datasets: {
      bar: {
        maxBarThickness: 48,
        borderRadius: 8
      }
    },
    scales: {
      x: {
        grid: {
          color: this.borderColor
        },
        ticks: {
          color: this.bodyColor
        }
      },
      y: {
        beginAtZero: true,
        min: 0,
        grid: {
          color: this.borderColor
        },
        ticks: {
          precision: 0,
          stepSize: 1,
          color: this.bodyColor
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

  ngAfterViewInit(): void {
    this.renderChart();
  }

  ngOnDestroy(): void {
    this.chartInstance?.destroy();
    this.chartInstance = null;
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
          const chartSeries = this.buildDailyOrderSeries(dashboard);
          this.chartLabels = chartSeries.labels;
          this.chartValues = chartSeries.values;
          this.renderChart();
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

  private buildDailyOrderSeries(dashboard: AdminDashboardDto): { labels: string[]; values: number[] } {
    const countsByDate = new Map<string, number>();

    for (const item of dashboard.dailyOrders ?? []) {
      if (!item?.date) {
        continue;
      }

      const key = String(item.date).slice(0, 10);
      countsByDate.set(key, item.ordersCount ?? 0);
    }

    const generatedAt = dashboard.generatedAt ? new Date(dashboard.generatedAt) : new Date();
    generatedAt.setHours(0, 0, 0, 0);

    const labels: string[] = [];
    const values: number[] = [];

    for (let offset = 6; offset >= 0; offset -= 1) {
      const day = new Date(generatedAt);
      day.setDate(generatedAt.getDate() - offset);

      const key = day.toISOString().slice(0, 10);
      labels.push(day.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' }));
      values.push(countsByDate.get(key) ?? 0);
    }

    return { labels, values };
  }

  private renderChart(): void {
    const canvas = this._ordersChartRef?.nativeElement;

    if (!canvas || this.chartLabels.length === 0) {
      return;
    }

    this.chartInstance?.destroy();

    this.chartInstance = new Chart(canvas, {
      type: 'bar',
      data: {
        labels: this.chartLabels,
        datasets: [
          {
            label: 'Đơn hàng',
            data: this.chartValues,
            backgroundColor: this.primaryColor
          }
        ]
      },
      options: this.chartOptions
    });
  }
}

import { Component, OnInit } from '@angular/core';
import { AsyncPipe, DatePipe, NgClass, NgFor, NgIf } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { BehaviorSubject, combineLatest, of } from 'rxjs';
import { catchError, finalize, switchMap, tap } from 'rxjs/operators';
import { OrderService } from '../../../../core/services/order.service';
import { AdminOrderDto, OrderStatusOption } from '../../../../core/models/order.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-order-list',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, AsyncPipe, DatePipe, FormsModule],
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.scss']
})
export class OrderListComponent implements OnInit {
  orders$ = new BehaviorSubject<AdminOrderDto[]>([]);
  loading$ = new BehaviorSubject<boolean>(false);
  statuses$ = new BehaviorSubject<OrderStatusOption[]>([]);

  currentPage$ = new BehaviorSubject<number>(1);
  pageSize$ = new BehaviorSubject<number>(10);
  search$ = new BehaviorSubject<string>('');
  statusFilter$ = new BehaviorSubject<number | null>(null);
  totalPages$ = new BehaviorSubject<number>(1);
  totalCount$ = new BehaviorSubject<number>(0);
  actionMessage$ = new BehaviorSubject<string | null>(null);

  Math = Math;

  // Status colors mapping
  readonly statusColors: { [key: string]: string } = {
    'Pending': 'warning',
    'Paid': 'info',
    'Shipped': 'primary',
    'Delivered': 'success',
    'Cancelled': 'danger'
  };

  // Status display names in Vietnamese
  readonly statusDisplayNames: { [key: string]: string } = {
    'Pending': 'Chờ xử lý',
    'Paid': 'Đã thanh toán',
    'Shipped': 'Đang giao',
    'Delivered': 'Đã giao',
    'Cancelled': 'Đã hủy'
  };

  constructor(
    private orderService: OrderService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    // Load order statuses for filter dropdown
    this.orderService.getOrderStatuses().subscribe({
      next: (statuses) => this.statuses$.next(statuses),
      error: () => this.statuses$.next([])
    });

    // Handle messages from navigation
    this.route.queryParamMap.subscribe(params => {
      if (params.get('updated') === '1') {
        this.actionMessage$.next('Cập nhật trạng thái đơn hàng thành công.');
      }

      if (params.keys.length > 0) {
        this.router.navigate([], {
          relativeTo: this.route,
          queryParams: {},
          replaceUrl: true
        });
      }
    });

    // Load orders when page, pageSize, search or status filter changes
    combineLatest([this.currentPage$, this.pageSize$, this.search$, this.statusFilter$]).pipe(
      tap(() => this.loading$.next(true)),
      switchMap(([page, pageSize, search, status]) =>
        this.orderService.getAdminOrders({ page, pageSize, search, status }).pipe(
          tap((res) => {
            this.totalPages$.next(res.totalPages || 0);
            this.totalCount$.next(res.totalCount || 0);
            this.orders$.next(res.items || []);
          }),
          catchError((err) => {
            console.error('Error loading orders:', err);
            this.actionMessage$.next('Không thể tải danh sách đơn hàng. Vui lòng thử lại.');
            return of({
              items: [],
              totalCount: 0,
              page: 1,
              pageSize,
              totalPages: 0
            });
          }),
          finalize(() => this.loading$.next(false))
        )
      )
    ).subscribe();
  }

  onSearch(term: string): void {
    this.search$.next(term);
    this.currentPage$.next(1);
  }

  onStatusChange(status: string): void {
    const statusValue = status === '' ? null : parseInt(status, 10);
    this.statusFilter$.next(statusValue);
    this.currentPage$.next(1);
  }

  onPageChange(page: number): void {
    const currentTotalPages = this.totalPages$.getValue();
    if (page >= 1 && page <= currentTotalPages) {
      this.currentPage$.next(page);
    }
  }

  getPageArray(): number[] {
    const totalPages = this.totalPages$.getValue();
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Pending':
        return 'badge bg-warning bg-opacity-10 text-warning border border-warning border-opacity-25';
      case 'Paid':
        return 'badge bg-info bg-opacity-10 text-info border border-info border-opacity-25';
      case 'Shipped':
        return 'badge bg-primary bg-opacity-10 text-primary border border-primary border-opacity-25';
      case 'Delivered':
        return 'badge bg-success bg-opacity-10 text-success border border-success border-opacity-25';
      case 'Cancelled':
        return 'badge bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25';
      default:
        return 'badge bg-secondary bg-opacity-10 text-secondary border border-secondary border-opacity-25';
    }
  }

  getStatusDisplayName(status: string): string {
    return this.statusDisplayNames[status] || status;
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(amount);
  }

  getPaymentMethodDisplay(method: string): string {
    switch ((method || '').toLowerCase()) {
      case 'cash':
        return 'Tiền mặt';
      case 'vnpay':
        return 'VNPay';
      default:
        return method;
    }
  }

  getPaymentBadgeClass(method: string): string {
    switch ((method || '').toLowerCase()) {
      case 'cash':
        return 'bg-secondary bg-opacity-10 text-secondary border border-secondary border-opacity-25';
      case 'vnpay':
        return 'bg-success bg-opacity-10 text-success border border-success border-opacity-25';
      default:
        return 'bg-light text-dark border';
    }
  }

  viewOrderDetail(id: number): void {
    this.router.navigate(['/admin/orders', id]);
  }
}

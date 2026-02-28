import { Component, OnInit } from '@angular/core';
import { AsyncPipe, DatePipe, NgFor, NgIf } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { BehaviorSubject, of } from 'rxjs';
import { catchError, finalize, switchMap, tap } from 'rxjs/operators';
import { OrderService } from '../../../../core/services/order.service';
import { AdminOrderDetailDto, OrderStatusOption, UpdateOrderStatusRequest } from '../../../../core/models/order.model';
import { FormsModule } from '@angular/forms';
import { resolvePlantImageUrl } from '../../../../core/utils/image-url.util';

@Component({
  selector: 'app-admin-order-detail',
  standalone: true,
  imports: [NgIf, NgFor, AsyncPipe, DatePipe, FormsModule],
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.scss']
})
export class OrderDetailComponent implements OnInit {
  readonly fallbackImageUrl = 'assets/images/plant-placeholder.svg';

  order$ = new BehaviorSubject<AdminOrderDetailDto | null>(null);
  loading$ = new BehaviorSubject<boolean>(false);
  updating$ = new BehaviorSubject<boolean>(false);
  statuses$ = new BehaviorSubject<OrderStatusOption[]>([]);
  error$ = new BehaviorSubject<string | null>(null);
  successMessage$ = new BehaviorSubject<string | null>(null);

  selectedStatus: number | null = null;
  statusReason: string = '';
  orderId: number = 0;

  // Status colors mapping
  readonly statusColors: { [key: string]: string } = {
    'Pending': 'warning',
    'Paid': 'info',
    'Shipped': 'primary',
    'Delivered': 'success',
    'Cancelled': 'danger'
  };

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
    this.orderId = parseInt(this.route.snapshot.paramMap.get('id') || '0', 10);
    
    if (!this.orderId) {
      this.error$.next('ID đơn hàng không hợp lệ');
      return;
    }

    this.loadOrderDetail();
    this.loadOrderStatuses();
  }

  loadOrderDetail(): void {
    this.loading$.next(true);
    this.error$.next(null);

    this.orderService.getAdminOrderDetail(this.orderId).pipe(
      tap((order) => {
        this.order$.next(order);
        // Set current status as default for dropdown
        const currentStatusObj = this.statuses$.getValue().find(s => s.name === order.status);
        if (currentStatusObj) {
          this.selectedStatus = currentStatusObj.value;
        }
      }),
      catchError((err) => {
        console.error('Error loading order detail:', err);
        this.error$.next('Không thể tải thông tin đơn hàng. Vui lòng thử lại.');
        return of(null);
      }),
      finalize(() => this.loading$.next(false))
    ).subscribe();
  }

  loadOrderStatuses(): void {
    this.orderService.getOrderStatuses().subscribe({
      next: (statuses) => {
        this.statuses$.next(statuses);
        // Update selected status after statuses are loaded
        const order = this.order$.getValue();
        if (order) {
          const currentStatusObj = statuses.find(s => s.name === order.status);
          if (currentStatusObj) {
            this.selectedStatus = currentStatusObj.value;
          }
        }
      },
      error: () => this.statuses$.next([])
    });
  }

  updateStatus(): void {
    const order = this.order$.getValue();
    if (!order || this.selectedStatus === null) return;

    // Check if status actually changed
    const currentStatusObj = this.statuses$.getValue().find(s => s.name === order.status);
    if (currentStatusObj && currentStatusObj.value === this.selectedStatus) {
      this.successMessage$.next('Trạng thái không thay đổi');
      return;
    }

    this.updating$.next(true);
    this.successMessage$.next(null);

    const request: UpdateOrderStatusRequest = {
      newStatus: this.selectedStatus,
      reason: this.statusReason || undefined
    };

    this.orderService.updateOrderStatus(this.orderId, request).pipe(
      tap((result) => {
        this.successMessage$.next('Cập nhật trạng thái thành công!');
        this.statusReason = ''; // Clear reason
        // Reload order detail to show updated status and history
        this.loadOrderDetail();
      }),
      catchError((err) => {
        console.error('Error updating status:', err);
        const errorMsg = err.error?.message || 'Cập nhật trạng thái thất bại. Vui lòng thử lại.';
        this.error$.next(errorMsg);
        return of(null);
      }),
      finalize(() => this.updating$.next(false))
    ).subscribe();
  }

  goBack(): void {
    this.router.navigate(['/admin/orders']);
  }

  getStatusBadgeClass(status: string): string {
    const color = this.statusColors[status] || 'secondary';
    return `badge bg-${color}`;
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

  getAvailableStatuses(currentStatus: string): OrderStatusOption[] {
    const allStatuses = this.statuses$.getValue();
    
    // Define allowed transitions
    const allowedTransitions: { [key: string]: string[] } = {
      'Pending': ['Paid', 'Cancelled'],
      'Paid': ['Shipped', 'Cancelled'],
      'Shipped': ['Delivered', 'Cancelled'],
      'Delivered': [],
      'Cancelled': []
    };

    const allowed = allowedTransitions[currentStatus] || [];
    
    // Return current status + allowed next statuses
    return allStatuses.filter(s => 
      s.name === currentStatus || allowed.includes(s.name)
    );
  }

  isFinalStatus(status: string): boolean {
    return status === 'Delivered' || status === 'Cancelled';
  }

  resolveImageUrl(imageUrl?: string | null): string {
    return resolvePlantImageUrl(imageUrl);
  }

  onImageError(event: Event): void {
    const target = event.target as HTMLImageElement | null;
    if (!target || target.src.endsWith(this.fallbackImageUrl)) {
      return;
    }

    target.src = this.fallbackImageUrl;
  }
}

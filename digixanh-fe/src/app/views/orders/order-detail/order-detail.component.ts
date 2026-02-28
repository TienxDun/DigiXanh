import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { CustomerOrderDetailDto } from '../../../core/models/order.model';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink
  ],
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.scss']
})
export class OrderDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly orderService = inject(OrderService);
  private readonly cdr = inject(ChangeDetectorRef);

  order: CustomerOrderDetailDto | null = null;
  isLoading = false;
  errorMessage = '';
  orderId = 0;

  // US20 - Cancel Order
  showCancelModal = false;
  isCancelling = false;
  isCancelled = false;
  cancelReason = '';
  cancelError = '';

  ngOnInit(): void {
    this.orderId = Number(this.route.snapshot.paramMap.get('id'));
    if (!this.orderId || isNaN(this.orderId)) {
      this.errorMessage = 'Mã đơn hàng không hợp lệ';
      return;
    }
    this.loadOrderDetail();
  }

  loadOrderDetail(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.orderService.getMyOrderDetail(this.orderId).subscribe({
      next: (order) => {
        this.order = order;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        if (error.status === 404) {
          this.errorMessage = 'Không tìm thấy đơn hàng hoặc bạn không có quyền xem đơn hàng này.';
        } else {
          this.errorMessage = 'Không thể tải thông tin đơn hàng. Vui lòng thử lại sau.';
        }
        this.isLoading = false;
        this.cdr.detectChanges();
        console.error('Error loading order detail:', error);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/orders']);
  }

  getStatusBadgeColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'warning';
      case 'paid':
        return 'info';
      case 'shipped':
        return 'primary';
      case 'delivered':
        return 'success';
      case 'cancelled':
        return 'danger';
      default:
        return 'secondary';
    }
  }

  getBootstrapStatusBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'bg-warning bg-opacity-10 text-warning border border-warning border-opacity-25';
      case 'paid':
        return 'bg-info bg-opacity-10 text-info border border-info border-opacity-25';
      case 'shipped':
        return 'bg-primary bg-opacity-10 text-primary border border-primary border-opacity-25';
      case 'delivered':
        return 'bg-success bg-opacity-10 text-success border border-success border-opacity-25';
      case 'cancelled':
        return 'bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25';
      default:
        return 'bg-secondary bg-opacity-10 text-secondary border border-secondary border-opacity-25';
    }
  }

  getStatusDisplayName(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'Chờ xử lý';
      case 'paid':
        return 'Đã thanh toán';
      case 'shipped':
        return 'Đang giao';
      case 'delivered':
        return 'Đã giao';
      case 'cancelled':
        return 'Đã hủy';
      default:
        return status;
    }
  }

  getPaymentMethodDisplay(method: string): string {
    switch (method.toLowerCase()) {
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

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(amount);
  }

  formatDate(date: Date | string): string {
    return new Date(date).toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  canCancel(): boolean {
    return this.order?.status.toLowerCase() === 'pending';
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = 'assets/images/plant-placeholder.svg';
  }

  // ============== US20 - Cancel Order Methods ==============

  openCancelModal(): void {
    this.showCancelModal = true;
    this.cancelReason = '';
    this.cancelError = '';
    this.cdr.detectChanges();
  }

  closeCancelModal(): void {
    this.showCancelModal = false;
    this.cdr.detectChanges();
  }

  confirmCancel(): void {
    if (this.isCancelling) return;

    this.isCancelling = true;
    this.cancelError = '';

    this.orderService.cancelOrder(this.orderId, this.cancelReason || undefined).subscribe({
      next: (result) => {
        this.isCancelling = false;
        this.isCancelled = true;
        this.showCancelModal = false;

        // Cập nhật lại order
        if (this.order) {
          this.order.status = 'Cancelled';
        }

        this.cdr.detectChanges();
      },
      error: (error) => {
        this.isCancelling = false;
        this.cancelError = error.error?.message || 'Có lỗi xảy ra khi hủy đơn hàng. Vui lòng thử lại sau.';
        this.cdr.detectChanges();
        console.error('Error cancelling order:', error);
      }
    });
  }
}

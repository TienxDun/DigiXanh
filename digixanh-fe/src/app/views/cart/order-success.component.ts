import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { OrderDetailDto } from '../../core/models/order.model';

@Component({
  selector: 'app-order-success',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './order-success.component.html',
  styleUrl: './order-success.component.scss'
})
export class OrderSuccessComponent implements OnInit {
  order: OrderDetailDto | null = null;
  isFromPaymentReturn = false;
  paymentStatus: 'success' | 'failed' | 'cancelled' | null = null;
  message = '';

  constructor(private readonly router: Router) {
    const navigation = this.router.getCurrentNavigation();
    const state = navigation?.extras?.state as { 
      order?: OrderDetailDto; 
      fromPaymentReturn?: boolean;
      paymentStatus?: 'success' | 'failed' | 'cancelled';
      message?: string;
    };
    
    this.order = state?.order ?? null;
    this.isFromPaymentReturn = state?.fromPaymentReturn ?? false;
    this.paymentStatus = state?.paymentStatus ?? null;
    this.message = state?.message ?? '';
  }

  ngOnInit(): void {
    // Nếu không có thông tin đơn hàng và không phải từ payment return, chuyển về trang chủ
    if (!this.order && !this.isFromPaymentReturn) {
      this.router.navigate(['/']);
    }
  }

  // TC08: Thanh toán thành công
  get isPaymentSuccess(): boolean {
    return this.paymentStatus === 'success';
  }

  // TC09: Thanh toán thất bại hoặc bị hủy
  get isPaymentFailed(): boolean {
    return this.paymentStatus === 'failed' || this.paymentStatus === 'cancelled';
  }

  // Thanh toán tiền mặt (không qua VNPay)
  get isCashPayment(): boolean {
    return !this.isFromPaymentReturn && this.order?.paymentMethod === 'Cash';
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  }

  formatDate(date: Date | string): string {
    return new Date(date).toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getPaymentMethodLabel(method: string): string {
    return method === 'VNPay' ? 'Thanh toán qua VNPay' : 'Tiền mặt khi nhận hàng';
  }

  getStatusLabel(status: string): string {
    const statusMap: { [key: string]: string } = {
      'Pending': 'Đang chờ xử lý',
      'Paid': 'Đã thanh toán',
      'Shipped': 'Đang giao hàng',
      'Delivered': 'Đã giao hàng',
      'Cancelled': 'Đã hủy'
    };
    return statusMap[status] ?? status;
  }

  getStatusClass(status: string): string {
    const classMap: { [key: string]: string } = {
      'Pending': 'text-warning',
      'Paid': 'text-success',
      'Shipped': 'text-info',
      'Delivered': 'text-success',
      'Cancelled': 'text-danger'
    };
    return classMap[status] ?? 'text-secondary';
  }

  // TC09: Lấy icon tương ứng với trạng thái
  get statusIcon(): string {
    if (this.isPaymentSuccess) return 'fa-check-circle';
    if (this.isPaymentFailed) return 'fa-times-circle';
    return 'fa-info-circle';
  }

  // TC09: Lấy màu tương ứng với trạng thái
  get statusColor(): string {
    if (this.isPaymentSuccess) return 'success';
    if (this.isPaymentFailed) return 'danger';
    return 'info';
  }

  // TC08/TC09: Tiêu đề trang
  get pageTitle(): string {
    if (this.isPaymentSuccess) return 'Thanh toán thành công!';
    if (this.isPaymentFailed) return 'Thanh toán thất bại';
    return 'Đặt hàng thành công!';
  }

  // TC08/TC09: Mô tả trang
  get pageDescription(): string {
    if (this.isPaymentSuccess) {
      return 'Cảm ơn bạn đã thanh toán. Đơn hàng của bạn đã được xác nhận và sẽ được xử lý ngay.';
    }
    if (this.isPaymentFailed) {
      return this.message || 'Rất tiếc, giao dịch của bạn không thể hoàn tất. Bạn có thể thử lại hoặc chọn phương thức thanh toán khác.';
    }
    return 'Cảm ơn bạn đã đặt hàng. Chúng tôi sẽ liên hệ để xác nhận đơn hàng trước khi giao.';
  }
}

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

  orderId: number | null = null;
  transactionId: string | null = null;

  // Timeline steps
  timelineSteps = [
    { status: 'Pending', label: 'Đặt hàng', icon: 'fa-shopping-bag', description: 'Đơn hàng đã được tạo' },
    { status: 'Paid', label: 'Xác nhận', icon: 'fa-check', description: 'Đã xác nhận thanh toán' },
    { status: 'Processing', label: 'Xử lý', icon: 'fa-cog', description: 'Đang chuẩn bị hàng' },
    { status: 'Shipped', label: 'Giao hàng', icon: 'fa-truck', description: 'Đang vận chuyển' },
    { status: 'Delivered', label: 'Hoàn tất', icon: 'fa-box-open', description: 'Đã giao hàng' }
  ];

  constructor(private readonly router: Router) {
    const navigation = this.router.getCurrentNavigation();
    const state = navigation?.extras?.state as { 
      order?: OrderDetailDto; 
      orderId?: number;
      fromPaymentReturn?: boolean;
      fromPaymentReturnComponent?: boolean;
      paymentStatus?: 'success' | 'failed' | 'cancelled';
      message?: string;
      transactionId?: string;
    };
    
    this.order = state?.order ?? null;
    this.orderId = state?.orderId ?? null;
    this.transactionId = state?.transactionId ?? null;
    this.isFromPaymentReturn = state?.fromPaymentReturn ?? state?.fromPaymentReturnComponent ?? false;
    this.paymentStatus = state?.paymentStatus ?? null;
    this.message = state?.message ?? '';
  }

  ngOnInit(): void {
    if (!this.order && !this.isFromPaymentReturn && !this.orderId) {
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

  // Thanh toán tiền mặt
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
      'Processing': 'Đang xử lý',
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
      'Processing': 'text-info',
      'Shipped': 'text-primary',
      'Delivered': 'text-success',
      'Cancelled': 'text-danger'
    };
    return classMap[status] ?? 'text-secondary';
  }

  get statusIcon(): string {
    if (this.isPaymentSuccess) return 'fa-check-circle';
    if (this.isPaymentFailed) return 'fa-times-circle';
    return 'fa-info-circle';
  }

  get statusColor(): string {
    if (this.isPaymentSuccess) return 'success';
    if (this.isPaymentFailed) return 'danger';
    return 'info';
  }

  get pageTitle(): string {
    if (this.isPaymentSuccess) return 'Thanh toán thành công!';
    if (this.isPaymentFailed) return 'Thanh toán thất bại';
    return 'Đặt hàng thành công!';
  }

  get pageDescription(): string {
    if (this.isPaymentSuccess) {
      return 'Cảm ơn bạn đã thanh toán. Đơn hàng của bạn đã được xác nhận và sẽ được xử lý ngay.';
    }
    if (this.isPaymentFailed) {
      return this.message || 'Rất tiếc, giao dịch của bạn không thể hoàn tất. Bạn có thể thử lại hoặc chọn phương thức thanh toán khác.';
    }
    return 'Cảm ơn bạn đã đặt hàng. Chúng tôi sẽ liên hệ để xác nhận đơn hàng trước khi giao.';
  }

  // Get current step index for timeline
  get currentStepIndex(): number {
    if (!this.order) return 0;
    const statusOrder = ['Pending', 'Paid', 'Processing', 'Shipped', 'Delivered'];
    const index = statusOrder.indexOf(this.order.status);
    return index >= 0 ? index : 0;
  }

  // Check if step is completed
  isStepCompleted(stepIndex: number): boolean {
    return stepIndex <= this.currentStepIndex;
  }

  // Check if step is active
  isStepActive(stepIndex: number): boolean {
    return stepIndex === this.currentStepIndex;
  }

  // Calculate estimated delivery date
  get estimatedDeliveryDate(): string {
    if (!this.order?.orderDate) return '';
    const orderDate = new Date(this.order.orderDate);
    const minDate = new Date(orderDate);
    const maxDate = new Date(orderDate);
    minDate.setDate(minDate.getDate() + 3);
    maxDate.setDate(maxDate.getDate() + 5);
    
    return `${minDate.toLocaleDateString('vi-VN')} - ${maxDate.toLocaleDateString('vi-VN')}`;
  }

  // Print order
  printOrder(): void {
    window.print();
  }

  // Download order as text
  downloadOrder(): void {
    if (!this.order) return;
    
    const content = `
================================
      ĐƠN HÀNG DIGIXANH
================================
Mã đơn hàng: #${this.order.id}
Ngày đặt: ${this.formatDate(this.order.orderDate)}

--- THÔNG TIN NGƯỜI NHẬN ---
Họ tên: ${this.order.recipientName}
SĐT: ${this.order.phone}
Địa chỉ: ${this.order.shippingAddress}

--- CHI TIẾT ĐƠN HÀNG ---
${this.order.items.map(item => `${item.plantName} x${item.quantity} - ${this.formatPrice(item.lineTotal)}`).join('\n')}

--- THANH TOÁN ---
Tạm tính: ${this.formatPrice(this.order.totalAmount)}
Giảm giá: ${this.order.discountAmount > 0 ? '-' + this.formatPrice(this.order.discountAmount) : '0 ₫'}
Tổng cộng: ${this.formatPrice(this.order.finalAmount)}
Phương thức: ${this.getPaymentMethodLabel(this.order.paymentMethod)}
${this.order.transactionId ? `Mã giao dịch: ${this.order.transactionId}` : ''}

================================
Cảm ơn bạn đã mua hàng!
Hotline: 1900 1234
================================
    `.trim();

    const blob = new Blob([content], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `don-hang-${this.order.id}.txt`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }

  // Copy order ID to clipboard
  copyOrderId(): void {
    if (this.order?.id) {
      navigator.clipboard.writeText(this.order.id.toString());
    }
  }
}

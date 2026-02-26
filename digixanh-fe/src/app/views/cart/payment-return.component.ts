import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OrderService } from '../../core/services/order.service';
import { OrderDetailDto } from '../../core/models/order.model';

@Component({
  selector: 'app-payment-return',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="container py-5 text-center">
      <div class="py-5">
        <div class="spinner-border text-success mb-3" role="status" style="width: 3rem; height: 3rem;">
          <span class="visually-hidden">Đang xử lý...</span>
        </div>
        <h4 class="fw-bold">Đang xử lý kết quả thanh toán...</h4>
        <p class="text-muted">Vui lòng chờ trong giây lát</p>
      </div>
    </section>
  `
})
export class PaymentReturnComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly orderService = inject(OrderService);
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    // Lấy tất cả query params từ URL
    const queryParams: { [key: string]: string } = {};
    
    this.route.queryParamMap.pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(params => {
      params.keys.forEach(key => {
        const value = params.get(key);
        if (value !== null) {
          queryParams[key] = value;
        }
      });

      this.processPaymentReturn(queryParams);
    });
  }

  private processPaymentReturn(vnpayData: { [key: string]: string }): void {
    const responseCode = vnpayData['vnp_ResponseCode'];
    const transactionStatus = vnpayData['vnp_TransactionStatus'];
    const orderId = vnpayData['vnp_TxnRef'];

    // Theo docs VNPay: thành công khi cả 2 mã đều = '00'
    const isSuccessFromUrl = responseCode === '00' && transactionStatus === '00';
    
    // Gọi API để xác nhận và cập nhật trạng thái đơn hàng
    this.orderService.processVNPayReturn(vnpayData)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          if (response.success) {
            // TC08: Thanh toán thành công - dùng order từ response nếu có
            const order = response.order ?? {
              id: response.orderId,
              orderDate: new Date(),
              totalAmount: 0,
              discountAmount: 0,
              finalAmount: 0,
              status: 'Paid',
              paymentMethod: 'VNPay',
              transactionId: response.transactionId || vnpayData['vnp_TransactionNo'],
              recipientName: '',
              phone: '',
              shippingAddress: '',
              items: []
            };

            this.router.navigate(['/order-success'], {
              state: { 
                order: order,
                fromPaymentReturn: true,
                paymentStatus: 'success' as const,
                message: response.message
              }
            });
          } else {
            // TC09: Thanh toán thất bại - dùng order từ response nếu có
            const paymentStatus = response.status === 'Cancelled' ? 'cancelled' : 'failed';

            this.router.navigate(['/order-success'], {
              state: { 
                order: response.order ?? null,
                fromPaymentReturn: true,
                paymentStatus: paymentStatus,
                message: response.message
              }
            });
          }
        },
        error: (error) => {
          // Lỗi khi gọi API - vẫn chuyển đến trang kết quả dựa trên response code từ URL
          if (isSuccessFromUrl) {
            const order: OrderDetailDto = {
              id: parseInt(orderId || '0'),
              orderDate: new Date(),
              totalAmount: 0,
              discountAmount: 0,
              finalAmount: 0,
              status: 'Paid',
              paymentMethod: 'VNPay',
              transactionId: vnpayData['vnp_TransactionNo'],
              recipientName: '',
              phone: '',
              shippingAddress: '',
              items: []
            };
            
            this.router.navigate(['/order-success'], {
              state: { 
                order: order,
                fromPaymentReturn: true,
                paymentStatus: 'success' as const,
                message: 'Thanh toán thành công.'
              }
            });
          } else {
            // TC09: Thanh toán thất bại hoặc đã hủy
            const status = responseCode === '24' ? 'cancelled' : 'failed';
            const message = this.getErrorMessage(responseCode, transactionStatus);
            
            this.router.navigate(['/order-success'], {
              state: { 
                order: null,
                fromPaymentReturn: true,
                paymentStatus: status,
                message: message
              }
            });
          }
        }
      });
  }

  // TC09: Lấy thông báo lỗi theo mã
  private getErrorMessage(responseCode?: string, transactionStatus?: string): string {
    const errorMessages: { [key: string]: string } = {
      '24': 'Đã hủy thanh toán.',
      '25': 'Giao dịch không tìm thấy.',
      '51': 'Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.',
      '65': 'Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.',
      '75': 'Ngân hàng thanh toán đang bảo trì.',
      '99': 'Lỗi không xác định từ ngân hàng.',
    };

    const transactionStatusMessages: { [key: string]: string } = {
      '01': 'Giao dịch chưa hoàn tất.',
      '02': 'Giao dịch bị lỗi.',
      '04': 'Giao dịch đảo.',
      '05': 'VNPay đang xử lý giao dịch.',
      '06': 'VNPay đã gửi yêu cầu hoàn tiền sang ngân hàng.',
      '07': 'Giao dịch bị nghi ngờ gian lận.',
      '09': 'Giao dịch hoàn trả bị từ chối.'
    };
    
    if (responseCode && errorMessages[responseCode]) {
      return errorMessages[responseCode];
    }

    if (transactionStatus && transactionStatusMessages[transactionStatus]) {
      return transactionStatusMessages[transactionStatus];
    }

    return `Thanh toán thất bại. Mã lỗi: ${responseCode ?? 'N/A'}`;
  }
}

import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OrderService } from '../../core/services/order.service';
import { OrderDetailDto } from '../../core/models/order.model';
import { take } from 'rxjs';

@Component({
  selector: 'app-payment-return',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="container py-5">
      <div class="row justify-content-center">
        <div class="col-md-8 col-lg-6">
          <div class="card border-0 shadow-lg">
            <div class="card-body p-4 p-lg-5 text-center">
              
              <!-- Loading State -->
              <div *ngIf="isLoading" class="py-4">
                <div class="spinner-border text-primary mb-3" role="status" style="width: 3.5rem; height: 3.5rem;">
                  <span class="visually-hidden">Đang xử lý...</span>
                </div>
                <h4 class="fw-bold">Đang xử lý kết quả thanh toán...</h4>
                <p class="text-muted mb-0">Vui lòng chờ trong giây lát</p>
              </div>

              <!-- Success State -->
              <div *ngIf="!isLoading && paymentStatus === 'success'">
                <div class="mb-4">
                  <div class="success-icon mx-auto">
                    <i class="fas fa-check"></i>
                  </div>
                </div>
                <h2 class="fw-bold text-success mb-3">Thanh toán thành công!</h2>
                <p class="text-muted mb-4">{{ message }}</p>

                <div class="bg-light rounded-3 p-4 text-start mb-4">
                  <div class="d-flex justify-content-between small mb-2">
                    <span class="text-muted">Mã đơn hàng</span>
                    <span class="fw-semibold">#{{ orderId }}</span>
                  </div>
                  <div *ngIf="transactionId" class="d-flex justify-content-between small">
                    <span class="text-muted">Mã giao dịch VNPay</span>
                    <span class="fw-semibold font-monospace">{{ transactionId }}</span>
                  </div>
                </div>

                <div class="d-grid gap-2">
                  <button class="btn btn-success btn-lg" (click)="viewOrderDetails()">
                    <i class="fas fa-receipt me-2"></i>
                    Xem chi tiết đơn hàng
                  </button>
                  <a routerLink="/plants" class="btn btn-outline-success">
                    <i class="fas fa-leaf me-2"></i>
                    Tiếp tục mua sắm
                  </a>
                </div>
              </div>

              <!-- Cancelled State -->
              <div *ngIf="!isLoading && paymentStatus === 'cancelled'">
                <div class="mb-4">
                  <div class="cancelled-icon mx-auto">
                    <i class="fas fa-ban"></i>
                  </div>
                </div>
                <h2 class="fw-bold text-warning mb-3">Đã hủy thanh toán</h2>
                <p class="text-muted mb-4">{{ message }}</p>

                <div class="alert alert-info d-flex align-items-start text-start">
                  <i class="fas fa-info-circle mt-1 me-2"></i>
                  <div>
                    <strong>Giỏ hàng của bạn đã được khôi phục.</strong><br>
                    Bạn có thể thử thanh toán lại hoặc chọn phương thức thanh toán khác.
                  </div>
                </div>

                <div class="d-grid gap-2 mt-4">
                  <a routerLink="/checkout" class="btn btn-success btn-lg">
                    <i class="fas fa-redo me-2"></i>
                    Thử thanh toán lại
                  </a>
                  <a routerLink="/cart" class="btn btn-outline-secondary">
                    <i class="fas fa-shopping-cart me-2"></i>
                    Quay lại giỏ hàng
                  </a>
                </div>
              </div>

              <!-- Failed State -->
              <div *ngIf="!isLoading && paymentStatus === 'failed'">
                <div class="mb-4">
                  <div class="failed-icon mx-auto">
                    <i class="fas fa-times"></i>
                  </div>
                </div>
                <h2 class="fw-bold text-danger mb-3">Thanh toán thất bại</h2>
                <p class="text-muted mb-4">{{ message }}</p>

                <div class="alert alert-warning d-flex align-items-start text-start" *ngIf="vnpResponseCode">
                  <i class="fas fa-exclamation-triangle mt-1 me-2"></i>
                  <div>
                    <strong>Mã lỗi:</strong> {{ vnpResponseCode }}<br>
                    <span class="small">Vui lòng kiểm tra thông tin thẻ hoặc thử phương thức thanh toán khác.</span>
                  </div>
                </div>

                <div class="d-grid gap-2 mt-4">
                  <a routerLink="/checkout" class="btn btn-success btn-lg">
                    <i class="fas fa-redo me-2"></i>
                    Thử lại
                  </a>
                  <a routerLink="/cart" class="btn btn-outline-secondary">
                    <i class="fas fa-shopping-cart me-2"></i>
                    Quay lại giỏ hàng
                  </a>
                </div>
              </div>

            </div>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .success-icon {
      width: 80px;
      height: 80px;
      background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 36px;
      box-shadow: 0 8px 24px rgba(40, 167, 69, 0.3);
    }

    .cancelled-icon {
      width: 80px;
      height: 80px;
      background: linear-gradient(135deg, #ffc107 0%, #ff9800 100%);
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 36px;
      box-shadow: 0 8px 24px rgba(255, 193, 7, 0.3);
    }

    .failed-icon {
      width: 80px;
      height: 80px;
      background: linear-gradient(135deg, #dc3545 0%, #f44336 100%);
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 36px;
      box-shadow: 0 8px 24px rgba(220, 53, 69, 0.3);
    }
  `]
})
export class PaymentReturnComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly orderService = inject(OrderService);
  private readonly destroyRef = inject(DestroyRef);

  isLoading = true;
  paymentStatus: 'success' | 'failed' | 'cancelled' = 'failed';
  message = 'Không nhận được dữ liệu thanh toán hợp lệ.';
  orderId: number | null = null;
  transactionId: string | null = null;
  order: OrderDetailDto | null = null;
  vnpResponseCode: string | null = null;

  ngOnInit(): void {
    this.route.queryParamMap
      .pipe(take(1), takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        const queryParams: { [key: string]: string } = {};
        params.keys.forEach(key => {
          const value = params.get(key);
          if (value !== null) {
            queryParams[key] = value;
          }
        });

        // Lưu response code để hiển thị
        this.vnpResponseCode = queryParams['vnp_ResponseCode'] || null;

        // Nếu BE đã xử lý và redirect với paymentStatus
        const normalizedStatus = queryParams['paymentStatus'];
        if (normalizedStatus === 'success' || normalizedStatus === 'failed' || normalizedStatus === 'cancelled') {
          this.paymentStatus = normalizedStatus;
          this.message = queryParams['message'] || this.getErrorMessage(queryParams['vnp_ResponseCode'], queryParams['vnp_TransactionStatus']);
          this.orderId = this.toNumber(queryParams['orderId']) ?? this.toNumber(queryParams['vnp_TxnRef']);
          this.transactionId = queryParams['transactionId'] || queryParams['vnp_TransactionNo'] || null;
          this.isLoading = false;
          
          // Nếu thanh toán thành công, lấy chi tiết đơn hàng
          if (this.paymentStatus === 'success' && this.orderId) {
            this.loadOrderDetails(this.orderId);
          }
          return;
        }

        // Nếu không có paymentStatus, kiểm tra VNPay params
        if (!queryParams['vnp_TxnRef']) {
          this.paymentStatus = 'failed';
          this.message = 'Không nhận được tham số giao dịch từ VNPay.';
          this.isLoading = false;
          return;
        }

        // Gọi API để xác thực với backend
        this.verifyWithBackend(queryParams);
      });
  }

  private loadOrderDetails(orderId: number): void {
    // Có thể gọi API để lấy chi tiết đơn hàng nếu cần
    // Hiện tại chỉ lưu orderId để redirect
  }

  viewOrderDetails(): void {
    if (this.order) {
      this.router.navigate(['/order-success'], {
        state: { 
          order: this.order,
          fromPaymentReturn: true,
          paymentStatus: 'success'
        }
      });
    } else if (this.orderId) {
      // Nếu chưa có order details, redirect với orderId
      this.router.navigate(['/order-success'], {
        state: { 
          orderId: this.orderId,
          fromPaymentReturn: true,
          paymentStatus: 'success',
          message: this.message,
          transactionId: this.transactionId
        }
      });
    } else {
      this.router.navigate(['/']);
    }
  }

  private verifyWithBackend(vnpayData: { [key: string]: string }): void {
    this.orderService.processVNPayReturn(vnpayData)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.order = response.order ?? null;
          this.orderId = response.orderId || this.toNumber(vnpayData['vnp_TxnRef']);
          this.transactionId = response.transactionId || vnpayData['vnp_TransactionNo'] || null;
          this.paymentStatus = response.success
            ? 'success'
            : (response.status === 'Cancelled' ? 'cancelled' : 'failed');
          this.message = response.message || this.getErrorMessage(vnpayData['vnp_ResponseCode'], vnpayData['vnp_TransactionStatus']);
          this.isLoading = false;
        },
        error: () => {
          const responseCode = vnpayData['vnp_ResponseCode'];
          const transactionStatus = vnpayData['vnp_TransactionStatus'];
          const isSuccess = responseCode === '00' && transactionStatus === '00';

          this.orderId = this.toNumber(vnpayData['vnp_TxnRef']);
          this.transactionId = vnpayData['vnp_TransactionNo'] || null;
          this.paymentStatus = isSuccess ? 'success' : (responseCode === '24' ? 'cancelled' : 'failed');
          this.message = isSuccess
            ? 'Thanh toán thành công! Cảm ơn bạn đã mua hàng.'
            : this.getErrorMessage(responseCode, transactionStatus);
          this.isLoading = false;
        }
      });
  }

  private toNumber(value?: string): number | null {
    if (!value) {
      return null;
    }

    const parsed = Number(value);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : null;
  }

  private getErrorMessage(responseCode?: string, transactionStatus?: string): string {
    const errorMessages: { [key: string]: string } = {
      '24': 'Bạn đã hủy giao dịch. Giỏ hàng của bạn đã được khôi phục.',
      '25': 'Giao dịch không tìm thấy.',
      '51': 'Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.',
      '65': 'Tài khoản của quý khách đã vượt quá hạn mức giao dịch trong ngày.',
      '75': 'Ngân hàng thanh toán đang bảo trì.',
      '99': 'Lỗi không xác định từ ngân hàng.'
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

import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OrderService } from '../../core/services/order.service';
import { OrderDetailDto } from '../../core/models/order.model';
import { timeout } from 'rxjs';

@Component({
  selector: 'app-payment-return',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="payment-return-container py-5 min-vh-100 d-flex align-items-center">
      <div class="container">
        <div class="row justify-content-center">
          <div class="col-md-8 col-lg-6">
            <div class="glass-card fade-in">
              <div class="card-body p-4 p-lg-5 text-center">
                
                <!-- Loading State -->
                <div *ngIf="isLoading" class="py-5">
                  <div class="loader-wrapper mb-4">
                    <div class="spinner-grow text-success" role="status"></div>
                    <div class="spinner-grow text-success-light mx-2" role="status"></div>
                    <div class="spinner-grow text-success" role="status"></div>
                  </div>
                  <h4 class="fw-bold dg-text-2xl mb-2">Đang xử lý thanh toán</h4>
                  <p class="text-muted">Vui lòng không đóng trình duyệt hoặc quay lại...</p>
                </div>

                <!-- Success State -->
                <div *ngIf="!isLoading && paymentStatus === 'success'" class="state-animation">
                  <div class="confetti-container"></div>
                  <div class="mb-4">
                    <div class="status-icon success-icon scale-in">
                      <i class="fas fa-check"></i>
                    </div>
                  </div>
                  <h2 class="fw-bold dg-text-4xl text-success mb-3">Thanh toán thành công!</h2>
                  <p class="dg-text-md text-muted mb-4 px-lg-4">
                    Tuyệt vời! Đơn hàng của bạn đã được xác nhận. Một email xác nhận sẽ được gửi đến bạn trong giây lát.
                  </p>

                  <div class="info-box mb-4 fade-in-up">
                    <div class="info-row">
                      <span class="label">Mã đơn hàng</span>
                      <span class="value">#{{ orderId }}</span>
                    </div>
                    <div *ngIf="transactionId" class="info-row">
                      <span class="label">Mã giao dịch</span>
                      <span class="value font-monospace">{{ transactionId }}</span>
                    </div>
                    <div class="info-divider"></div>
                    <div class="info-row">
                      <span class="label">Trạng thái</span>
                      <span class="badge bg-success-subtle text-success px-3 rounded-pill">Đã thanh toán</span>
                    </div>
                  </div>

                  <div class="d-grid gap-3">
                    <button class="btn btn-primary btn-lg dg-bold shadow-sm" (click)="viewOrderDetails()">
                      <i class="fas fa-receipt me-2"></i>
                      Xem chi tiết đơn hàng
                    </button>
                    <a routerLink="/plants" class="btn btn-outline-success border-2 dg-semibold">
                      <i class="fas fa-shopping-bag me-2"></i>
                      Tiếp tục mua hàng
                    </a>
                  </div>
                </div>

                <!-- Cancelled State -->
                <div *ngIf="!isLoading && paymentStatus === 'cancelled'" class="state-animation">
                  <div class="mb-4">
                    <div class="status-icon warning-icon scale-in">
                      <i class="fas fa-exclamation"></i>
                    </div>
                  </div>
                  <h2 class="fw-bold dg-text-4xl text-warning mb-3">Đã hủy thanh toán</h2>
                  <p class="dg-text-md text-muted mb-4 px-lg-4">
                    Bạn đã dừng quá trình thanh toán. Đừng lo, giỏ hàng của bạn vẫn được giữ nguyên.
                  </p>

                  <div class="alert alert-soft-warning mb-4 fade-in-up">
                    <div class="d-flex">
                      <i class="fas fa-info-circle mt-1 me-3"></i>
                      <div class="text-start">
                        <p class="mb-0 dg-text-sm">Bạn có thể thử lại ngay bây giờ hoặc đổi sang phương thức khác như <strong>Thanh toán khi nhận hàng (COD)</strong>.</p>
                      </div>
                    </div>
                  </div>

                  <div class="d-grid gap-3">
                    <a routerLink="/checkout" class="btn btn-warning btn-lg dg-bold text-white shadow-sm">
                      <i class="fas fa-redo me-2"></i>
                      Thử thanh toán lại
                    </a>
                    <a routerLink="/cart" class="btn btn-link text-secondary text-decoration-none dg-semibold">
                      <i class="fas fa-arrow-left me-2"></i>
                      Quay lại giỏ hàng
                    </a>
                  </div>
                </div>

                <!-- Failed State -->
                <div *ngIf="!isLoading && paymentStatus === 'failed'" class="state-animation">
                  <div class="mb-4">
                    <div class="status-icon danger-icon scale-in">
                      <i class="fas fa-times"></i>
                    </div>
                  </div>
                  <h2 class="fw-bold dg-text-4xl text-danger mb-3">Thanh toán thất bại</h2>
                  <p class="dg-text-md text-muted mb-4 px-lg-4">
                    Rất tiếc, đã có lỗi xảy ra trong quá trình giao dịch với ngân hàng.
                  </p>

                  <div class="error-details mb-4 fade-in-up">
                    <div class="d-flex align-items-center mb-2">
                       <span class="badge bg-danger-subtle text-danger me-2">Lỗi: {{ vnpResponseCode || 'Unknown' }}</span>
                       <span class="dg-text-sm text-secondary">{{ message }}</span>
                    </div>
                  </div>

                  <div class="support-contact mb-4 p-3 bg-light rounded-3 fade-in-up">
                    <p class="dg-text-xs text-muted mb-2 uppercase">Cần hỗ trợ ngay?</p>
                    <div class="d-flex justify-content-center gap-4">
                      <a href="tel:19001234" class="text-decoration-none text-dark dg-semibold">
                        <i class="fas fa-phone-alt text-success me-1"></i> 1900 1234
                      </a>
                      <a href="mailto:support@digixanh.vn" class="text-decoration-none text-dark dg-semibold">
                        <i class="fas fa-envelope text-success me-1"></i> support@digixanh.vn
                      </a>
                    </div>
                  </div>

                  <div class="d-grid gap-3">
                    <a routerLink="/checkout" class="btn btn-danger btn-lg dg-bold shadow-sm">
                      <i class="fas fa-sync-alt me-2"></i>
                      Thử lại lần nữa
                    </a>
                    <a routerLink="/cart" class="btn btn-outline-secondary border-2 dg-semibold">
                      Quay lại giỏ hàng
                    </a>
                  </div>
                </div>

              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: [`
    :host {
      --primary-green: #2e7d32;
      --light-green: #e8f5e9;
      --glass-bg: rgba(255, 255, 255, 0.9);
      --card-shadow: 0 20px 40px rgba(0, 0, 0, 0.08);
    }

    .payment-return-container {
      background: linear-gradient(135deg, #f8fcf9 0%, #e0f2f1 100%);
      position: relative;
      overflow: hidden;
    }

    .payment-return-container::before {
      content: '';
      position: absolute;
      top: -10%;
      right: -10%;
      width: 400px;
      height: 400px;
      background: radial-gradient(circle, rgba(46, 125, 50, 0.05) 0%, transparent 70%);
      border-radius: 50%;
    }

    .glass-card {
      background: var(--glass-bg);
      backdrop-filter: blur(10px);
      -webkit-backdrop-filter: blur(10px);
      border: 1px solid rgba(255, 255, 255, 0.5);
      border-radius: 24px;
      box-shadow: var(--card-shadow);
    }

    .status-icon {
      width: 100px;
      height: 100px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 40px;
      margin: 0 auto;
      position: relative;
    }

    .success-icon {
      background: linear-gradient(135deg, #4caf50 0%, #2e7d32 100%);
      color: white;
      box-shadow: 0 10px 25px rgba(76, 175, 80, 0.3);
    }

    .success-icon::after {
      content: '';
      position: absolute;
      width: 100%;
      height: 100%;
      border-radius: 50%;
      border: 2px solid #4caf50;
      animation: pulse-ring 2s infinite;
    }

    .warning-icon {
      background: linear-gradient(135deg, #ffc107 0%, #ff9800 100%);
      color: white;
      box-shadow: 0 10px 25px rgba(255, 193, 7, 0.3);
    }

    .danger-icon {
      background: linear-gradient(135deg, #ff5252 0%, #d32f2f 100%);
      color: white;
      box-shadow: 0 10px 25px rgba(211, 47, 47, 0.3);
    }

    .info-box {
      background: #fdfdfd;
      border: 1px solid #f0f0f0;
      border-radius: 16px;
      padding: 20px;
    }

    .info-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 12px;
    }

    .info-row:last-child {
      margin-bottom: 0;
    }

    .info-row .label {
      color: #757575;
      font-size: 14px;
    }

    .info-row .value {
      font-weight: 600;
      color: #212121;
    }

    .info-divider {
      height: 1px;
      background: #f0f0f0;
      margin: 15px 0;
    }

    .alert-soft-warning {
      background-color: #fff9c4;
      border: none;
      color: #5d4037;
      border-radius: 12px;
    }

    .btn {
      padding: 12px 24px;
      border-radius: 12px;
      transition: all 0.3s ease;
    }

    .btn-primary {
      background-color: var(--primary-green);
      border-color: var(--primary-green);
    }

    .btn-primary:hover {
      background-color: #1b5e20;
      transform: translateY(-2px);
      box-shadow: 0 8px 15px rgba(46, 125, 50, 0.2);
    }

    /* Animations */
    .fade-in {
      animation: fadeIn 0.8s ease-out;
    }

    .scale-in {
      animation: scaleIn 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275);
    }

    .fade-in-up {
      animation: fadeInUp 0.6s ease-out both;
      animation-delay: 0.3s;
    }

    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    @keyframes scaleIn {
      from { transform: scale(0.5); opacity: 0; }
      to { transform: scale(1); opacity: 1; }
    }

    @keyframes fadeInUp {
      from { transform: translateY(20px); opacity: 0; }
      to { transform: translateY(0); opacity: 1; }
    }

    @keyframes pulse-ring {
      0% { transform: scale(0.8); opacity: 1; }
      100% { transform: scale(1.4); opacity: 0; }
    }

    .loader-wrapper {
      display: flex;
      justify-content: center;
    }

    .text-success-light {
      color: #81c784;
    }

    .uppercase {
      text-transform: uppercase;
      letter-spacing: 1px;
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
  private hasProcessedQuery = false;

  ngOnInit(): void {
    const snapshotParams = this.route.snapshot.queryParams as { [key: string]: string };
    if (Object.keys(snapshotParams).length > 0) {
      this.handleQueryParams(snapshotParams);
      return;
    }

    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        if (this.hasProcessedQuery || params.keys.length === 0) {
          return;
        }

        const queryParams: { [key: string]: string } = {};
        params.keys.forEach(key => {
          const value = params.get(key);
          if (value !== null) {
            queryParams[key] = value;
          }
        });

        this.handleQueryParams(queryParams);
      });

    setTimeout(() => {
      if (!this.hasProcessedQuery && this.isLoading) {
        this.paymentStatus = 'failed';
        this.message = 'Không nhận được dữ liệu phản hồi từ VNPay. Vui lòng thử lại.';
        this.isLoading = false;
      }
    }, 15000);
  }

  private handleQueryParams(queryParams: { [key: string]: string }): void {
    this.hasProcessedQuery = true;

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

    const responseCode = queryParams['vnp_ResponseCode'];
    const transactionStatus = queryParams['vnp_TransactionStatus'];
    const hasVnPayStatus = !!responseCode || !!transactionStatus;

    // Hiển thị nhanh trạng thái dựa trên query từ VNPay để tránh cảm giác chờ lâu.
    if (hasVnPayStatus) {
      const isSuccess = responseCode === '00' && transactionStatus === '00';
      this.orderId = this.toNumber(queryParams['vnp_TxnRef']);
      this.transactionId = queryParams['vnp_TransactionNo'] || null;
      this.paymentStatus = isSuccess
        ? 'success'
        : (responseCode === '24' ? 'cancelled' : 'failed');
      this.message = isSuccess
        ? 'Đã nhận kết quả từ VNPay. Đang xác thực giao dịch...'
        : this.getErrorMessage(responseCode, transactionStatus);
      this.isLoading = false;
    }

    // Gọi API để xác thực với backend
    this.verifyWithBackend(queryParams);
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
      .pipe(
        timeout(8000),
        takeUntilDestroyed(this.destroyRef)
      )
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

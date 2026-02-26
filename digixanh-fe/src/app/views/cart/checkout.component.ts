import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CartItemDto, CartSummaryDto } from '../../core/models/cart.model';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { CreateOrderRequest, PaymentMethod } from '../../core/models/order.model';
import { resolvePlantImageUrl } from '../../core/utils/image-url.util';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss'
})
export class CheckoutComponent implements OnInit {
  readonly fallbackImageUrl = 'assets/images/plant-placeholder.svg';
  
  private readonly destroyRef = inject(DestroyRef);
  private readonly cartService = inject(CartService);
  private readonly orderService = inject(OrderService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  cartSummary: CartSummaryDto | null = null;
  checkoutForm!: FormGroup;
  isLoading = true;
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';

  // Thông tin giảm giá
  discountInfo: { baseAmount: number; discountAmount: number; discountPercent: number } | null = null;

  // Phương thức thanh toán
  paymentMethods = [
    { value: PaymentMethod.Cash, label: 'Tiền mặt khi nhận hàng', icon: 'fa-money-bill', description: 'Thanh toán bằng tiền mặt khi nhận được hàng' },
    { value: PaymentMethod.VNPay, label: 'Thanh toán qua VNPay', icon: 'fa-credit-card', description: 'Thanh toán trực tuyến qua cổng VNPay' }
  ];

  ngOnInit(): void {
    this.initForm();
    this.loadCart();
  }

  get hasItems(): boolean {
    return (this.cartSummary?.items.length ?? 0) > 0;
  }

  get totalQuantity(): number {
    return this.cartSummary?.totalQuantity ?? 0;
  }

  get baseAmount(): number {
    return this.cartSummary?.totalAmount ?? 0;
  }

  get discountAmount(): number {
    return this.discountInfo?.discountAmount ?? 0;
  }

  get finalAmount(): number {
    return this.baseAmount - this.discountAmount;
  }

  get hasDiscount(): boolean {
    return this.discountAmount > 0;
  }

  get selectedPaymentMethod(): PaymentMethod {
    return this.checkoutForm?.get('paymentMethod')?.value ?? PaymentMethod.Cash;
  }

  get isCashPayment(): boolean {
    return this.selectedPaymentMethod === PaymentMethod.Cash;
  }

  get isVNPayPayment(): boolean {
    return this.selectedPaymentMethod === PaymentMethod.VNPay;
  }

  private initForm(): void {
    this.checkoutForm = this.fb.group({
      recipientName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
      phone: ['', [Validators.required, Validators.pattern(/^[0-9]{10,11}$/)]],
      shippingAddress: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
      paymentMethod: [PaymentMethod.Cash, Validators.required]
    });
  }

  private loadCart(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.cartService.getCart()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (summary) => {
          this.cartSummary = summary;
          this.calculateDiscount();
          this.isLoading = false;

          // Nếu giỏ hàng trống, chuyển về trang giỏ hàng
          if (!this.hasItems) {
            this.router.navigate(['/cart']);
          }
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error?.error?.message ?? 'Không tải được giỏ hàng. Vui lòng thử lại.';
        }
      });
  }

  private calculateDiscount(): void {
    if (!this.cartSummary) {
      this.discountInfo = null;
      return;
    }

    const totalQuantity = this.cartSummary.items.reduce((sum, item) => sum + item.quantity, 0);
    const baseAmount = this.cartSummary.totalAmount;
    let discountPercent = 0;

    if (totalQuantity >= 3) {
      discountPercent = 7; // Giảm 7% nếu mua >= 3 sản phẩm
    } else if (totalQuantity >= 2) {
      discountPercent = 5; // Giảm 5% nếu mua >= 2 sản phẩm
    }

    const discountAmount = baseAmount * (discountPercent / 100);

    this.discountInfo = {
      baseAmount,
      discountAmount,
      discountPercent
    };
  }

  onSubmit(): void {
    if (this.checkoutForm.invalid) {
      this.markFormGroupTouched(this.checkoutForm);
      return;
    }

    if (!this.hasItems) {
      this.errorMessage = 'Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi đặt hàng.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request: CreateOrderRequest = {
      ...this.checkoutForm.value,
      returnUrl: `${window.location.origin}/payment-return`
    };

    this.orderService.createOrder(request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.isSubmitting = false;

          if (!response.success) {
            this.errorMessage = response.message;
            return;
          }

          // Cập nhật số lượng giỏ hàng về 0
          this.cartService.resetCartCount();

          if (this.isVNPayPayment && response.paymentUrl) {
            // Chuyển hướng đến VNPay
            window.location.href = response.paymentUrl;
          } else {
            // Thanh toán tiền mặt - chuyển đến trang thành công
            this.router.navigate(['/order-success'], {
              state: { order: response.order }
            });
          }
        },
        error: (error) => {
          this.isSubmitting = false;
          this.errorMessage = error?.error?.message ?? 'Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại.';
        }
      });
  }

  onImageError(event: Event): void {
    const target = event.target as HTMLImageElement | null;
    if (!target || target.src.endsWith(this.fallbackImageUrl)) {
      return;
    }
    target.src = this.fallbackImageUrl;
  }

  resolveImageUrl(imageUrl?: string | null): string {
    return resolvePlantImageUrl(imageUrl);
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  }

  getDiscountLabel(): string {
    if (!this.discountInfo) return '';
    const percent = this.discountInfo.discountPercent;
    if (percent === 7) return 'Giảm 7% (mua từ 3 sản phẩm)';
    if (percent === 5) return 'Giảm 5% (mua từ 2 sản phẩm)';
    return '';
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      if ((control as any).controls) {
        this.markFormGroupTouched(control as FormGroup);
      }
    });
  }
}

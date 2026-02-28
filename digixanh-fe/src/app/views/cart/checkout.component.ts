import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CartItemDto, CartSummaryDto } from '../../core/models/cart.model';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { UserService, UserProfile } from '../../core/services/user.service';
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
  private readonly userService = inject(UserService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  cartSummary: CartSummaryDto | null = null;
  checkoutForm!: FormGroup;
  isLoading = true;
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';
  userProfile: UserProfile | null = null;

  // Thông tin giảm giá - lấy từ CartSummaryDto
  discountInfo: { baseAmount: number; discountAmount: number; discountPercent: number } | null = null;

  // Phương thức thanh toán
  paymentMethods = [
    { value: PaymentMethod.Cash, label: 'Tiền mặt khi nhận hàng', icon: 'fa-money-bill', description: 'Thanh toán bằng tiền mặt khi nhận được hàng' },
    { value: PaymentMethod.VNPay, label: 'Thanh toán qua VNPay', icon: 'fa-credit-card', description: 'Thanh toán trực tuyến qua cổng VNPay' }
  ];

  ngOnInit(): void {
    this.initForm();
    this.loadCart();
    this.loadUserProfile(); // Tự động điền thông tin từ profile
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
    return this.cartSummary?.finalAmount ?? (this.baseAmount - this.discountAmount);
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

  get hasOutOfStockItems(): boolean {
    return this.cartSummary?.items.some(item => item.stockQuantity === 0) ?? false;
  }

  get outOfStockItemNames(): string {
    return this.cartSummary?.items
      .filter(item => item.stockQuantity === 0)
      .map(item => this.getItemName(item))
      .join(', ') ?? '';
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

  private loadUserProfile(): void {
    this.userService.getProfile().subscribe({
      next: (profile) => {
        this.userProfile = profile;
        // Tự động điền thông tin từ profile vào form checkout
        if (this.checkoutForm) {
          this.checkoutForm.patchValue({
            recipientName: profile.fullName || '',
            phone: profile.phoneNumber || '',
            shippingAddress: profile.address || ''
          });
        }
      },
      error: (error) => {
        // Không hiển thị lỗi - ngườI dùng có thể tự nhập
        console.log('Không thể tải profile:', error);
      }
    });
  }

  private calculateDiscount(): void {
    if (!this.cartSummary) {
      this.discountInfo = null;
      return;
    }

    // Sử dụng discount info từ CartSummaryDto (đã được tính toán ở backend hoặc cart component)
    const baseAmount = this.cartSummary.totalAmount;
    const discountAmount = this.cartSummary.discountAmount ?? 0;
    const discountPercent = this.cartSummary.discountPercent ?? 0;

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

    if (this.hasOutOfStockItems) {
      this.errorMessage = `Không thể đặt hàng. Sản phẩm ${this.outOfStockItemNames} đã hết hàng. Vui lòng xóa khỏi giỏ hàng hoặc quay lại sau.`;
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request: CreateOrderRequest = {
      ...this.checkoutForm.value
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

  getItemName(item: CartItemDto): string {
    return item.plantName?.trim() || `Sản phẩm #${item.plantId}`;
  }

  trackByItemId(_index: number, item: CartItemDto): number {
    return item.id;
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

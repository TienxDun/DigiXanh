import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CartItemDto, CartSummaryDto } from '../../core/models/cart.model';
import { CartService } from '../../core/services/cart.service';
import { resolvePlantImageUrl } from '../../core/utils/image-url.util';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.scss'
})
export class CartComponent {
  readonly fallbackImageUrl = 'assets/images/plant-placeholder.svg';

  private readonly destroyRef = inject(DestroyRef);
  private readonly cartService = inject(CartService);
  private readonly router = inject(Router);

  cartSummary: CartSummaryDto | null = null;
  isLoading = true;
  isCheckingOut = false;
  errorMessage = '';

  private readonly processingItemIds = new Set<number>();

  constructor() {
    this.loadCart();
  }

  get hasItems(): boolean {
    return (this.cartSummary?.items.length ?? 0) > 0;
  }

  isItemProcessing(itemId: number): boolean {
    return this.processingItemIds.has(itemId);
  }

  increaseQuantity(item: CartItemDto): void {
    if (item.quantity >= 99 || this.isItemProcessing(item.id)) {
      return;
    }

    this.updateQuantity(item, item.quantity + 1);
  }

  decreaseQuantity(item: CartItemDto): void {
    if (item.quantity <= 1 || this.isItemProcessing(item.id)) {
      return;
    }

    this.updateQuantity(item, item.quantity - 1);
  }

  onQuantityInput(item: CartItemDto, value: string): void {
    if (this.isItemProcessing(item.id)) {
      return;
    }

    const parsedValue = Number(value);
    if (Number.isNaN(parsedValue)) {
      return;
    }

    const quantity = Math.max(1, Math.min(99, Math.trunc(parsedValue)));
    if (quantity === item.quantity) {
      return;
    }

    this.updateQuantity(item, quantity);
  }

  removeItem(item: CartItemDto): void {
    if (this.isItemProcessing(item.id)) {
      return;
    }

    this.errorMessage = '';
    const previous = this.cartSummary;
    if (!previous) {
      return;
    }

    this.processingItemIds.add(item.id);

    this.cartSummary = this.toSummary(previous.items.filter(current => current.id !== item.id));

    this.cartService.removeCartItem(item.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (summary) => {
          this.cartSummary = summary;
          this.processingItemIds.delete(item.id);
        },
        error: (error) => {
          this.processingItemIds.delete(item.id);
          this.cartSummary = previous;
          this.errorMessage = error?.error?.message ?? 'Không thể xóa sản phẩm khỏi giỏ hàng.';
        }
      });
  }

  proceedToCheckout(): void {
    this.isCheckingOut = true;
    this.router.navigate(['/checkout'])
      .finally(() => {
        this.isCheckingOut = false;
      });
  }

  trackByItemId(_index: number, item: CartItemDto): number {
    return item.id;
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

  private loadCart(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.cartService.getCart()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (summary) => {
          this.cartSummary = summary;
          this.isLoading = false;
        },
        error: (error) => {
          this.isLoading = false;
          this.cartSummary = null;
          this.errorMessage = error?.error?.message ?? 'Không tải được giỏ hàng. Vui lòng thử lại.';
        }
      });
  }

  private updateQuantity(item: CartItemDto, quantity: number): void {
    const previous = this.cartSummary;
    if (!previous) {
      return;
    }

    this.errorMessage = '';
    this.processingItemIds.add(item.id);

    const updatedItems = previous.items.map((current) => {
      if (current.id !== item.id) {
        return current;
      }

      return {
        ...current,
        quantity,
        lineTotal: current.price * quantity
      };
    });

    this.cartSummary = this.toSummary(updatedItems);

    this.cartService.updateCartItemQuantity(item.id, { quantity })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (summary) => {
          this.cartSummary = summary;
          this.processingItemIds.delete(item.id);
        },
        error: (error) => {
          this.processingItemIds.delete(item.id);
          this.cartSummary = previous;
          this.errorMessage = error?.error?.message ?? 'Không thể cập nhật số lượng.';
        }
      });
  }

  private toSummary(items: CartItemDto[]): CartSummaryDto {
    const totalQuantity = items.reduce((sum, item) => sum + item.quantity, 0);
    const totalAmount = items.reduce((sum, item) => sum + item.lineTotal, 0);

    return {
      items,
      totalQuantity,
      totalAmount
    };
  }
}

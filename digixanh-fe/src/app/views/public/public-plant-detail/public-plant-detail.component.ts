import { ChangeDetectorRef, Component, NgZone, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, filter, map, Observable, of, switchMap, tap } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { PlantDetailDto, PlantDto } from '../../../core/models/plant.model';
import { PublicPlantService } from '../../../core/services/public-plant.service';
import { AuthService } from '../../../core/services/auth.service';
import { CartService } from '../../../core/services/cart.service';
import { ToastService } from '../../../core/services/toast.service';
import { resolvePlantImageUrl } from '../../../core/utils/image-url.util';

@Component({
  selector: 'app-public-plant-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './public-plant-detail.component.html',
  styleUrl: './public-plant-detail.component.scss'
})
export class PublicPlantDetailComponent implements OnDestroy {
  readonly fallbackImageUrl = 'assets/images/plant-placeholder.svg';

  quantity = 1;
  isAdding = false;
  errorMessage = '';
  successMessage = '';
  isSuccess = false;
  activeTab: 'description' | 'care' | 'shipping' = 'description';

  private currentPlantId: number | null = null;
  currentPlant: PlantDetailDto | null = null;
  private successResetTimer: ReturnType<typeof setTimeout> | null = null;

  readonly plant$: Observable<PlantDetailDto | null>;
  readonly relatedPlants$: Observable<PlantDto[]>;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly publicPlantService: PublicPlantService,
    private readonly authService: AuthService,
    private readonly cartService: CartService,
    private readonly toastService: ToastService,
    private readonly ngZone: NgZone,
    private readonly cdr: ChangeDetectorRef
  ) {
    this.plant$ = this.route.paramMap.pipe(
      map(params => Number(params.get('id'))),
      switchMap((id) => {
        if (!id || Number.isNaN(id)) {
          this.errorMessage = 'ID cây không hợp lệ.';
          return of(null);
        }

        this.currentPlantId = id;
        this.errorMessage = '';

        return this.publicPlantService.getPlantDetail(id).pipe(
          tap((plant) => {
            this.successMessage = '';
            this.currentPlant = plant;
            this.quantity = 1;
          }),
          catchError(() => {
            this.errorMessage = 'Không tải được thông tin chi tiết cây.';
            return of(null);
          })
        );
      })
    );

    // Sản phẩm liên quan: chạy sau khi plant$ emit để lấy categoryId
    this.relatedPlants$ = this.plant$.pipe(
      filter((p): p is PlantDetailDto => p !== null),
      switchMap(plant =>
        this.publicPlantService.getRelatedPlants(plant.categoryId, plant.id, 4).pipe(
          map(result => result.items.filter(p => p.id !== plant.id).slice(0, 4)),
          catchError(() => of([]))
        )
      )
    );
  }

  setTab(tab: 'description' | 'care' | 'shipping'): void {
    this.activeTab = tab;
  }

  increaseQuantity(): void {
    const maxQuantity = this.getMaxQuantity();
    if (this.quantity < maxQuantity) {
      this.quantity += 1;
    }
  }

  getMaxQuantity(): number {
    if (!this.currentPlant?.stockQuantity) {
      return 99;
    }
    return Math.min(99, this.currentPlant.stockQuantity);
  }

  isOutOfStock(): boolean {
    return this.currentPlant?.stockQuantity === 0;
  }

  decreaseQuantity(): void {
    if (this.quantity > 1) {
      this.quantity -= 1;
    }
  }

  onQuantityInput(value: string): void {
    const parsed = Number(value);
    if (Number.isNaN(parsed)) {
      this.quantity = 1;
      return;
    }

    const maxQuantity = this.getMaxQuantity();
    this.quantity = Math.max(1, Math.min(maxQuantity, Math.trunc(parsed)));
  }

  addToCart(): void {
    this.errorMessage = '';
    this.successMessage = '';
    this.clearSuccessTimer();

    if (!this.currentPlantId) {
      this.errorMessage = 'Không xác định được cây cần thêm vào giỏ.';
      return;
    }

    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login'], {
        queryParams: { returnUrl: this.router.url }
      });
      return;
    }

    if (this.isOutOfStock()) {
      this.errorMessage = 'Sản phẩm đã hết hàng.';
      return;
    }

    this.isAdding = true;
    this.cartService.addToCart({
      plantId: this.currentPlantId,
      quantity: this.quantity
    }).subscribe({
      next: () => {
        this.isAdding = false;
        this.isSuccess = true;
        this.cdr.detectChanges();
        this.toastService.success('Đã thêm sản phẩm vào giỏ hàng', 'Thành công', 2500);
        this.successResetTimer = setTimeout(() => {
          this.ngZone.run(() => {
            this.isSuccess = false;
            this.cdr.detectChanges();
          });
        }, 2000);
      },
      error: (error) => {
        this.isAdding = false;
        this.toastService.error(error?.error?.message ?? 'Có lỗi xảy ra', 'Thất bại');
      }
    });
  }

  ngOnDestroy(): void {
    this.clearSuccessTimer();
  }

  onImageError(event: Event): void {
    const target = event.target as HTMLImageElement | null;
    if (!target || target.src.endsWith(this.fallbackImageUrl)) return;
    target.src = this.fallbackImageUrl;
  }

  resolveImageUrl(imageUrl?: string | null): string {
    return resolvePlantImageUrl(imageUrl);
  }

  hasRealImage(imageUrl?: string | null): boolean {
    return Boolean(imageUrl && imageUrl.trim().length > 0);
  }

  /** Kiểm tra số lượng tồn kho để hiển thị badge phù hợp */
  getStockStatus(qty?: number | null): 'in-stock' | 'low-stock' | 'out-of-stock' {
    if (qty === null || qty === undefined) return 'in-stock';
    if (qty === 0) return 'out-of-stock';
    if (qty <= 10) return 'low-stock';
    return 'in-stock';
  }

  private clearSuccessTimer(): void {
    if (this.successResetTimer) {
      clearTimeout(this.successResetTimer);
      this.successResetTimer = null;
    }
  }
}


import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, map, Observable, of, switchMap, tap } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { PlantDetailDto } from '../../../core/models/plant.model';
import { PublicPlantService } from '../../../core/services/public-plant.service';
import { AuthService } from '../../../core/services/auth.service';
import { CartService } from '../../../core/services/cart.service';
import { resolvePlantImageUrl } from '../../../core/utils/image-url.util';

@Component({
  selector: 'app-public-plant-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './public-plant-detail.component.html',
  styleUrl: './public-plant-detail.component.scss'
})
export class PublicPlantDetailComponent {
  readonly fallbackImageUrl = 'assets/images/plant-placeholder.svg';

  quantity = 1;
  isAdding = false;
  errorMessage = '';
  successMessage = '';

  private currentPlantId: number | null = null;

  readonly plant$: Observable<PlantDetailDto | null>;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly publicPlantService: PublicPlantService,
    private readonly authService: AuthService,
    private readonly cartService: CartService
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
          tap(() => {
            this.successMessage = '';
          }),
          catchError(() => {
            this.errorMessage = 'Không tải được thông tin chi tiết cây.';
            return of(null);
          })
        );
      })
    );
  }

  increaseQuantity(): void {
    if (this.quantity < 99) {
      this.quantity += 1;
    }
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

    this.quantity = Math.max(1, Math.min(99, Math.trunc(parsed)));
  }

  addToCart(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.currentPlantId) {
      this.errorMessage = 'Không xác định được cây cần thêm vào giỏ.';
      return;
    }

    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login'], {
        queryParams: {
          returnUrl: this.router.url
        }
      });
      return;
    }

    this.isAdding = true;
    this.cartService.addToCart({
      plantId: this.currentPlantId,
      quantity: this.quantity
    }).subscribe({
      next: () => {
        this.isAdding = false;
        this.successMessage = 'Đã thêm cây vào giỏ hàng thành công.';
      },
      error: (error) => {
        this.isAdding = false;
        this.errorMessage = error?.error?.message ?? 'Không thể thêm vào giỏ hàng. Vui lòng thử lại.';
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
}
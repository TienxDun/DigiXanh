import { ChangeDetectionStrategy, Component, NgZone, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Observable, Subscription, catchError, interval, map, of } from 'rxjs';
import { PlantDto, CategoryDto } from '../../../core/models/plant.model';
import { PublicPlantService } from '../../../core/services/public-plant.service';
import { resolvePlantImageUrl } from '../../../core/utils/image-url.util';

interface BannerSlide {
  id: number;
  title: string;
  subtitle: string;
  description: string;
  imageUrl: string;
  ctaText: string;
  ctaLink: string;
  bgGradient: string;
}

import { PlantCardComponent } from '../../../shared/components/plant-card/plant-card.component';

@Component({
  selector: 'app-homepage',
  standalone: true,
  imports: [CommonModule, RouterLink, PlantCardComponent],
  templateUrl: './homepage.component.html',
  styleUrl: './homepage.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomepageComponent {
  private readonly plantService = inject(PublicPlantService);
  private readonly ngZone = inject(NgZone);
  readonly fallbackImageUrl = 'assets/images/plant-placeholder.svg';

  // Hero slider data
  readonly bannerSlides: BannerSlide[] = [
    {
      id: 1,
      title: 'Không Gian Xanh',
      subtitle: 'Cho Cuộc Sống Hiện Đại',
      description: 'Mang những tác phẩm nghệ thuật từ thiên nhiên về với không gian của bạn. Lựa chọn những loại cây chất lượng nhất từ vườn ươm của chúng tôi.',
      imageUrl: 'https://images.unsplash.com/photo-1463936575829-25148e1db1b8?w=800',
      ctaText: 'Khám phá ngay',
      ctaLink: '/plants',
      bgGradient: 'linear-gradient(135deg, #e8f5e9 0%, #c8e6c9 100%)'
    },
    {
      id: 2,
      title: 'Cây Cảnh Độc Đáo',
      subtitle: 'Tạo Điểm Nhấn Khác Biệt',
      description: 'Những loài cây quý hiếm và độc đáo, được chăm sóc kỹ lưỡng để mang đến vẻ đẹp tự nhiên hoàn hảo cho không gian sống của bạn.',
      imageUrl: 'https://images.unsplash.com/photo-1416879595882-3373a0480b5b?w=800',
      ctaText: 'Xem bộ sưu tập',
      ctaLink: '/plants',
      bgGradient: 'linear-gradient(135deg, #f1f8e9 0%, #dcedc8 100%)'
    },
    {
      id: 3,
      title: 'Dễ Dàng Chăm Sóc',
      subtitle: 'Phù Hợp Mọi Không Gian',
      description: 'Từ cây cảnh mini để bàn đến những chậu cây lớn cho văn phòng, tất cả đều được hướng dẫn chăm sóc chi tiết.',
      imageUrl: 'https://images.unsplash.com/photo-1459156212016-c812468e2115?w=800',
      ctaText: 'Tìm cây phù hợp',
      ctaLink: '/plants',
      bgGradient: 'linear-gradient(135deg, #e0f2f1 0%, #b2dfdb 100%)'
    }
  ];

  currentSlide = 0;
  private slideIntervalSub: Subscription | null = null;

  // Swipe gesture support
  private touchStartX = 0;
  private touchEndX = 0;
  private readonly minSwipeDistance = 50; // pixels

  // Featured products - state driven for loadMore support
  private readonly INITIAL_LOAD_COUNT = 8;
  private readonly PAGE_SIZE = 4;
  private allFeaturedPlants = signal<PlantDto[]>([]);
  private currentDisplayCount = signal<number>(this.INITIAL_LOAD_COUNT);
  private totalAvailable = signal<number>(0);
  private nextServerPage = signal<number>(2);
  readonly isLoadingMore = signal<boolean>(false);
  readonly isInitialLoading = signal<boolean>(true);

  readonly featuredPlants = computed(() =>
    this.allFeaturedPlants().slice(0, this.currentDisplayCount())
  );

  readonly hasMorePlants = computed(() =>
    this.currentDisplayCount() < this.allFeaturedPlants().length ||
    this.allFeaturedPlants().length < this.totalAvailable()
  );

  // Categories
  readonly categories$: Observable<CategoryDto[]> = this.plantService.getCategories().pipe(
    map(categories => categories.slice(0, 6)), // Show first 6 categories
    catchError(() => of([]))
  );

  // Benefits data
  readonly benefits = [
    {
      icon: 'fa-truck-fast',
      title: 'Giao Hàng Nhanh',
      description: 'Giao hàng toàn quốc trong 2-3 ngày'
    },
    {
      icon: 'fa-shield-halved',
      title: 'Bảo Hành Cây',
      description: 'Đổi trả trong 7 ngày nếu cây có vấn đề'
    },
    {
      icon: 'fa-headset',
      title: 'Tư Vấn 24/7',
      description: 'Đội ngũ chuyên gia sẵn sàng hỗ trợ'
    },
    {
      icon: 'fa-tag',
      title: 'Giá Tốt Nhất',
      description: 'Cam kết giá cả cạnh tranh nhất thị trường'
    }
  ];

  ngOnInit(): void {
    this.startAutoSlide();
    this.loadInitialPlants();
  }

  private loadInitialPlants(): void {
    this.plantService.getPlants({ page: 1, pageSize: this.INITIAL_LOAD_COUNT, sortBy: '' }).pipe(
      catchError(() => of({ items: [], totalCount: 0, page: 1, pageSize: this.INITIAL_LOAD_COUNT, totalPages: 0 }))
    ).subscribe(result => {
      this.allFeaturedPlants.set(this.mergeUniquePlants([], result.items ?? []));
      this.totalAvailable.set(result.totalCount);
      const loadedCount = result.items?.length ?? 0;
      this.nextServerPage.set(Math.floor(loadedCount / this.PAGE_SIZE) + 1);
      this.isInitialLoading.set(false);
    });
  }

  loadMorePlants(): void {
    if (this.isLoadingMore()) return;

    const nextDisplayCount = this.currentDisplayCount() + this.PAGE_SIZE;

    // If we already have enough loaded items, just show more
    if (nextDisplayCount <= this.allFeaturedPlants().length) {
      this.currentDisplayCount.set(nextDisplayCount);
      return;
    }

    if (this.allFeaturedPlants().length >= this.totalAvailable()) {
      return;
    }

    // Otherwise fetch another page from the server
    this.isLoadingMore.set(true);
    const nextPage = this.nextServerPage();
    this.plantService.getPlants({ page: nextPage, pageSize: this.PAGE_SIZE, sortBy: '' }).pipe(
      map(result => result.items ?? []),
      catchError(() => of([]))
    ).subscribe(newItems => {
      this.allFeaturedPlants.update(current => this.mergeUniquePlants(current, newItems));
      if (newItems.length > 0) {
        this.nextServerPage.update((page) => page + 1);
      }
      this.currentDisplayCount.set(Math.min(nextDisplayCount, this.allFeaturedPlants().length));
      this.isLoadingMore.set(false);
    });
  }

  private mergeUniquePlants(current: PlantDto[], incoming: PlantDto[]): PlantDto[] {
    if (!incoming.length) {
      return current;
    }

    const seenIds = new Set(current.map((plant) => plant.id));
    const merged = [...current];

    for (const plant of incoming) {
      if (seenIds.has(plant.id)) {
        continue;
      }

      seenIds.add(plant.id);
      merged.push(plant);
    }

    return merged;
  }

  ngOnDestroy(): void {
    this.stopAutoSlide();
  }

  private startAutoSlide(): void {
    this.stopAutoSlide();
    this.ngZone.runOutsideAngular(() => {
      this.slideIntervalSub = interval(5000).subscribe(() => {
        this.ngZone.run(() => this.nextSlide());
      });
    });
  }

  private stopAutoSlide(): void {
    this.slideIntervalSub?.unsubscribe();
    this.slideIntervalSub = null;
  }

  nextSlide(): void {
    this.currentSlide = (this.currentSlide + 1) % this.bannerSlides.length;
  }

  prevSlide(): void {
    this.currentSlide = (this.currentSlide - 1 + this.bannerSlides.length) % this.bannerSlides.length;
  }

  goToSlide(index: number): void {
    this.currentSlide = index;
    this.resetSlideTimer();
  }

  private resetSlideTimer(): void {
    this.stopAutoSlide();
    this.startAutoSlide();
  }

  // Swipe Action Handlers
  onTouchStart(event: TouchEvent): void {
    this.touchStartX = event.touches[0].clientX;
    this.touchEndX = this.touchStartX;
    this.stopAutoSlide();
  }

  onTouchMove(event: TouchEvent): void {
    this.touchEndX = event.touches[0].clientX;
  }

  onTouchEnd(): void {
    const swipeDistance = this.touchStartX - this.touchEndX;

    if (Math.abs(swipeDistance) > this.minSwipeDistance) {
      if (swipeDistance > 0) {
        this.nextSlide();
      } else {
        this.prevSlide();
      }
    }

    this.startAutoSlide();
  }

  resolveImageUrl(imageUrl?: string | null): string {
    return resolvePlantImageUrl(imageUrl);
  }

  onImageError(event: Event): void {
    const target = event.target as HTMLImageElement | null;
    if (!target || target.src.endsWith(this.fallbackImageUrl)) {
      return;
    }
    target.src = this.fallbackImageUrl;
  }

  // Category images mapping
  getCategoryImage(categoryName: string): string {
    const categoryImages: Record<string, string> = {
      'Cây trong nhà': 'https://images.unsplash.com/photo-1485955900006-10f4d324d411?w=400',
      'Cây văn phòng': 'https://images.unsplash.com/photo-1501004318641-b39e6451bec6?w=400',
      'Cây để bàn': 'https://images.unsplash.com/photo-1453904300235-0f2f60b15b5d?w=400',
      'Cây cao cấp': 'https://images.unsplash.com/photo-1459411552884-841db9b3cc2a?w=400',
      'Sen đá': 'https://images.unsplash.com/photo-1509423350716-97f9360b4e09?w=400',
      'Cây nước': 'https://images.unsplash.com/photo-1463320898484-cdee8141c787?w=400'
    };
    return categoryImages[categoryName] || 'https://images.unsplash.com/photo-1463936575829-25148e1db1b8?w=400';
  }
}

import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Observable, catchError, map, of } from 'rxjs';
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

@Component({
  selector: 'app-homepage',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './homepage.component.html',
  styleUrl: './homepage.component.scss'
})
export class HomepageComponent {
  private readonly plantService = inject(PublicPlantService);
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
  private slideInterval: any;

  // Featured products - state driven for loadMore support
  private readonly PAGE_SIZE = 4;
  private allFeaturedPlants = signal<PlantDto[]>([]);
  private currentDisplayCount = signal<number>(8);
  private totalAvailable = signal<number>(0);
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
    // Load 12 plants upfront to support multiple "load more" cycles
    this.plantService.getPlants({ page: 1, pageSize: 12, sortBy: '' }).pipe(
      catchError(() => of({ items: [], totalCount: 0, page: 1, pageSize: 12, totalPages: 0 }))
    ).subscribe(result => {
      this.allFeaturedPlants.set(result.items);
      this.totalAvailable.set(result.totalCount);
      this.isInitialLoading.set(false);
    });
  }

  loadMorePlants(): void {
    if (this.isLoadingMore()) return;

    const next = this.currentDisplayCount() + this.PAGE_SIZE;

    // If we already have enough loaded items, just show more
    if (next <= this.allFeaturedPlants().length) {
      this.currentDisplayCount.set(next);
      return;
    }

    // Otherwise fetch another page from the server
    this.isLoadingMore.set(true);
    const nextPage = Math.floor(this.allFeaturedPlants().length / this.PAGE_SIZE) + 1;
    this.plantService.getPlants({ page: nextPage, pageSize: this.PAGE_SIZE, sortBy: '' }).pipe(
      map(result => result.items ?? []),
      catchError(() => of([]))
    ).subscribe(newItems => {
      this.allFeaturedPlants.update(current => [...current, ...newItems]);
      this.currentDisplayCount.set(this.allFeaturedPlants().length);
      this.isLoadingMore.set(false);
    });
  }

  ngOnDestroy(): void {
    this.stopAutoSlide();
  }

  private startAutoSlide(): void {
    this.slideInterval = setInterval(() => {
      this.nextSlide();
    }, 5000);
  }

  private stopAutoSlide(): void {
    if (this.slideInterval) {
      clearInterval(this.slideInterval);
    }
  }

  nextSlide(): void {
    this.currentSlide = (this.currentSlide + 1) % this.bannerSlides.length;
  }

  prevSlide(): void {
    this.currentSlide = (this.currentSlide - 1 + this.bannerSlides.length) % this.bannerSlides.length;
  }

  goToSlide(index: number): void {
    this.currentSlide = index;
    // Reset interval when manually changing slide
    this.stopAutoSlide();
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

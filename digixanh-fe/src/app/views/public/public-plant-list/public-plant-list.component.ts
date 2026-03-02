import { ChangeDetectionStrategy, ChangeDetectorRef, Component, DestroyRef, NgZone, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { asyncScheduler, catchError, fromEvent, Observable, of, throttleTime } from 'rxjs';
import { CategoryDto, PlantDto } from '../../../core/models/plant.model';
import { PagedResult } from '../../../core/models/pagination.model';
import { PublicPlantService } from '../../../core/services/public-plant.service';
import { resolvePlantImageUrl } from '../../../core/utils/image-url.util';

interface PublicPlantListVm {
  items: PlantDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Component({
  selector: 'app-public-plant-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './public-plant-list.component.html',
  styleUrl: './public-plant-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PublicPlantListComponent implements OnInit {
  readonly fallbackImageUrl = 'assets/images/plant-placeholder.svg';
  private readonly destroyRef = inject(DestroyRef);
  private readonly ngZone = inject(NgZone);
  private readonly cdr = inject(ChangeDetectorRef);
  private latestRequestId = 0;

  private readonly pageSize = 24;

  private currentPage = 1;
  private currentSearch = '';
  private currentCategoryId: number | null = null;
  private currentSortBy = '';

  isInitialLoading = false;
  isLoadingMore = false;

  searchInput = '';
  selectedCategory = '';
  selectedSort = '';

  readonly categories$: Observable<CategoryDto[]>;
  vm: PublicPlantListVm = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: this.pageSize,
    totalPages: 0
  };

  readonly sortOptions = [
    { value: '', label: 'Mặc định (Mới nhất)' },
    { value: 'priceAsc', label: 'Giá tăng dần' },
    { value: 'priceDesc', label: 'Giá giảm dần' },
    { value: 'nameAsc', label: 'Tên A-Z' }
  ];

  constructor(
    private publicPlantService: PublicPlantService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.categories$ = this.publicPlantService.getCategories().pipe(
      catchError(() => of([]))
    );
  }

  ngOnInit(): void {
    this.setupInfiniteScroll();

    this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params: any) => {
      const category = params['category'];
      const parsedCategory = Number(category);

      if (category && !Number.isNaN(parsedCategory) && parsedCategory > 0) {
        this.selectedCategory = category;
        this.currentCategoryId = parsedCategory;
      } else {
        this.selectedCategory = '';
        this.currentCategoryId = null;
      }

      this.resetAndLoadPlants();
      this.cdr.markForCheck();
    });
  }

  onSearch(): void {
    this.currentSearch = this.searchInput.trim();
    this.resetAndLoadPlants();
  }

  onCategoryChange(value: string): void {
    const normalizedValue = value?.trim() ?? '';

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { category: normalizedValue || null },
      queryParamsHandling: 'merge'
    });
  }

  onSortChange(value: string): void {
    this.currentSortBy = value;
    this.resetAndLoadPlants();
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

  private resetAndLoadPlants(): void {
    this.currentPage = 1;
    this.vm = {
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: this.pageSize,
      totalPages: 0
    };

    this.loadPlants(false);
  }

  private loadMorePlants(): void {
    this.currentPage += 1;
    this.loadPlants(true);
  }

  private loadPlants(append: boolean): void {
    const requestId = ++this.latestRequestId;

    if (append) {
      this.isLoadingMore = true;
    } else {
      this.isInitialLoading = true;
    }

    this.cdr.markForCheck();

    this.publicPlantService.getPlants({
      page: this.currentPage,
      pageSize: this.pageSize,
      search: this.currentSearch,
      categoryId: this.currentCategoryId,
      sortBy: this.currentSortBy
    }).pipe(
      catchError(() => of({
        items: [],
        totalCount: 0,
        page: this.currentPage,
        pageSize: this.pageSize,
        totalPages: 0
      } as PagedResult<PlantDto>)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((result: PagedResult<PlantDto>) => {
      if (requestId !== this.latestRequestId) {
        return;
      }

      const nextItems = result.items ?? [];
      const mergedItems = append ? [...this.vm.items, ...nextItems] : nextItems;

      this.vm = {
        items: mergedItems,
        totalCount: result.totalCount ?? 0,
        page: result.page ?? this.currentPage,
        pageSize: result.pageSize ?? this.pageSize,
        totalPages: result.totalPages ?? 0
      };

      this.isInitialLoading = false;
      this.isLoadingMore = false;
      this.cdr.markForCheck();
    });
  }

  private setupInfiniteScroll(): void {
    this.ngZone.runOutsideAngular(() => {
      fromEvent(window, 'scroll', { passive: true }).pipe(
        throttleTime(150, asyncScheduler, { leading: true, trailing: true }),
        takeUntilDestroyed(this.destroyRef)
      ).subscribe(() => {
        if (this.isInitialLoading || this.isLoadingMore || this.vm.page >= this.vm.totalPages) {
          return;
        }

        const scrollPosition = window.innerHeight + window.scrollY;
        const threshold = document.body.offsetHeight - 300;

        if (scrollPosition >= threshold) {
          this.ngZone.run(() => this.loadMorePlants());
        }
      });
    });
  }
}

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { BehaviorSubject, catchError, combineLatest, map, Observable, of, switchMap } from 'rxjs';
import { CategoryDto, PlantDto } from '../../../core/models/plant.model';
import { PagedResult } from '../../../core/models/pagination.model';
import { PublicPlantService } from '../../../core/services/public-plant.service';

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
  styleUrl: './public-plant-list.component.scss'
})
export class PublicPlantListComponent {
  private readonly pageSize = 12;

  private readonly currentPage$ = new BehaviorSubject<number>(1);
  private readonly search$ = new BehaviorSubject<string>('');
  private readonly categoryId$ = new BehaviorSubject<number | null>(null);
  private readonly sortBy$ = new BehaviorSubject<string>('');

  searchInput = '';
  selectedCategory = '';
  selectedSort = '';

  readonly categories$: Observable<CategoryDto[]>;
  readonly vm$: Observable<PublicPlantListVm>;

  readonly sortOptions = [
    { value: '', label: 'Mặc định (Mới nhất)' },
    { value: 'priceAsc', label: 'Giá tăng dần' },
    { value: 'priceDesc', label: 'Giá giảm dần' },
    { value: 'nameAsc', label: 'Tên A-Z' }
  ];

  constructor(private publicPlantService: PublicPlantService) {
    this.categories$ = this.publicPlantService.getCategories().pipe(
      catchError(() => of([]))
    );

    this.vm$ = combineLatest([
      this.currentPage$,
      this.search$,
      this.categoryId$,
      this.sortBy$
    ]).pipe(
      switchMap(([page, search, categoryId, sortBy]) =>
        this.publicPlantService.getPlants({
          page,
          pageSize: this.pageSize,
          search,
          categoryId,
          sortBy
        }).pipe(
          map((result: PagedResult<PlantDto>) => ({
            items: result.items ?? [],
            totalCount: result.totalCount ?? 0,
            page: result.page ?? 1,
            pageSize: result.pageSize ?? this.pageSize,
            totalPages: result.totalPages ?? 0
          })),
          catchError(() => of({
            items: [],
            totalCount: 0,
            page,
            pageSize: this.pageSize,
            totalPages: 0
          }))
        )
      )
    );
  }

  onSearch(): void {
    this.currentPage$.next(1);
    this.search$.next(this.searchInput.trim());
  }

  onCategoryChange(value: string): void {
    this.currentPage$.next(1);
    this.categoryId$.next(value ? Number(value) : null);
  }

  onSortChange(value: string): void {
    this.currentPage$.next(1);
    this.sortBy$.next(value);
  }

  goToPage(page: number, totalPages: number): void {
    if (page < 1 || page > totalPages) {
      return;
    }

    this.currentPage$.next(page);
  }

  getPageRange(totalPages: number): number[] {
    return Array.from({ length: totalPages }, (_, index) => index + 1);
  }
}
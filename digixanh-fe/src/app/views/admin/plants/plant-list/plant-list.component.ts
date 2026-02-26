import { Component, OnInit } from '@angular/core';
import { AsyncPipe, DatePipe, DecimalPipe, NgFor, NgIf } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BehaviorSubject, combineLatest, of } from 'rxjs';
import { catchError, finalize, switchMap, tap } from 'rxjs/operators';
import { AdminPlantService } from '../../../../core/services/admin-plant.service';
import { PlantDto } from '../../../../core/models/plant.model';

@Component({
  selector: 'app-admin-plant-list',
  standalone: true,
  imports: [NgIf, NgFor, AsyncPipe, DecimalPipe, DatePipe, RouterLink],
  templateUrl: './plant-list.component.html',
  styleUrls: ['./plant-list.component.scss']
})
export class PlantListComponent implements OnInit {
  plants$ = new BehaviorSubject<PlantDto[]>([]);
  loading$ = new BehaviorSubject<boolean>(false);

  currentPage$ = new BehaviorSubject<number>(1);
  pageSize$ = new BehaviorSubject<number>(10);
  search$ = new BehaviorSubject<string>('');
  totalPages$ = new BehaviorSubject<number>(1);
  totalCount$ = new BehaviorSubject<number>(0);
  actionMessage$ = new BehaviorSubject<string | null>(null);
  selectedIds$ = new BehaviorSubject<Set<number>>(new Set<number>());

  Math = Math;

  constructor(
    private adminPlantService: AdminPlantService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      if (params.get('updated') === '1') {
        this.actionMessage$.next('Cập nhật cây thành công.');
      } else if (params.get('created') === '1') {
        this.actionMessage$.next('Thêm cây thành công.');
      } else if (params.get('deleted') === '1') {
        this.actionMessage$.next('Xoá cây thành công.');
      }

      if (params.keys.length > 0) {
        this.router.navigate([], {
          relativeTo: this.route,
          queryParams: {},
          replaceUrl: true
        });
      }
    });

    combineLatest([this.currentPage$, this.pageSize$, this.search$]).pipe(
      tap(() => this.loading$.next(true)),
      switchMap(([page, pageSize, search]) =>
        this.adminPlantService.getPlants(page, pageSize, search).pipe(
          tap((res) => {
            this.totalPages$.next(res.totalPages || 0);
            this.totalCount$.next(res.totalCount || 0);
            this.plants$.next(res.items || []);

            const validIds = new Set((res.items || []).map(item => item.id));
            const nextSelection = new Set<number>(
              [...this.selectedIds$.getValue()].filter(id => validIds.has(id))
            );
            this.selectedIds$.next(nextSelection);
          }),
          catchError(() => {
            return of({
              items: [],
              totalCount: 0,
              page: 1,
              pageSize,
              totalPages: 0
            });
          }),
          finalize(() => this.loading$.next(false))
        )
      )
    ).subscribe();
  }

  onSearch(term: string): void {
    this.search$.next(term);
    this.currentPage$.next(1); // Reset to first page
  }

  onPageChange(page: number): void {
    const currentTotalPages = this.totalPages$.getValue();
    if (page >= 1 && page <= currentTotalPages) {
      this.currentPage$.next(page);
    }
  }

  deletePlant(id: number): void {
    if (confirm('Bạn có chắc chắn muốn xoá?')) {
      this.loading$.next(true);
      this.adminPlantService.deletePlant(id).pipe(
        finalize(() => this.loading$.next(false))
      ).subscribe({
        next: () => {
          this.actionMessage$.next('Xoá cây thành công.');
          const nextSelection = new Set(this.selectedIds$.getValue());
          nextSelection.delete(id);
          this.selectedIds$.next(nextSelection);
          this.currentPage$.next(this.currentPage$.getValue());
        },
        error: () => {
          this.actionMessage$.next('Xoá cây thất bại. Vui lòng thử lại.');
        }
      });
    }
  }

  isSelected(id: number): boolean {
    return this.selectedIds$.getValue().has(id);
  }

  toggleSelection(id: number, checked: boolean): void {
    const nextSelection = new Set(this.selectedIds$.getValue());
    if (checked) {
      nextSelection.add(id);
    } else {
      nextSelection.delete(id);
    }

    this.selectedIds$.next(nextSelection);
  }

  isAllSelected(plants: PlantDto[]): boolean {
    if (plants.length === 0) {
      return false;
    }

    const selected = this.selectedIds$.getValue();
    return plants.every(plant => selected.has(plant.id));
  }

  isAllSelectedCurrentPage(): boolean {
    return this.isAllSelected(this.plants$.getValue());
  }

  toggleSelectAll(plants: PlantDto[], checked: boolean): void {
    const nextSelection = new Set(this.selectedIds$.getValue());

    for (const plant of plants) {
      if (checked) {
        nextSelection.add(plant.id);
      } else {
        nextSelection.delete(plant.id);
      }
    }

    this.selectedIds$.next(nextSelection);
  }

  toggleSelectAllCurrentPage(checked: boolean): void {
    this.toggleSelectAll(this.plants$.getValue(), checked);
  }

  bulkDeleteSelected(): void {
    const ids = [...this.selectedIds$.getValue()];
    if (ids.length === 0) {
      this.actionMessage$.next('Bạn chưa chọn cây nào để xoá.');
      return;
    }

    if (!confirm(`Bạn có chắc chắn muốn xoá ${ids.length} cây đã chọn?`)) {
      return;
    }

    this.loading$.next(true);
    this.adminPlantService.deletePlantsBulk(ids).pipe(
      finalize(() => this.loading$.next(false))
    ).subscribe({
      next: () => {
        this.actionMessage$.next(`Đã xoá ${ids.length} cây thành công.`);
        this.selectedIds$.next(new Set<number>());
        this.currentPage$.next(this.currentPage$.getValue());
      },
      error: () => {
        this.actionMessage$.next('Xoá hàng loạt thất bại. Vui lòng thử lại.');
      }
    });
  }

  getPageArray(): number[] {
    const totalPages = this.totalPages$.getValue();
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }
}

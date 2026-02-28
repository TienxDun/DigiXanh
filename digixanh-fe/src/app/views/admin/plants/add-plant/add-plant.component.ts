import {
    Component,
    OnInit,
    OnDestroy,
    ChangeDetectionStrategy,
    ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormlyModule, FormlyFieldConfig } from '@ngx-formly/core';
import { FormlyBootstrapModule } from '@ngx-formly/bootstrap';
import {
    Subject,
    BehaviorSubject,
    from,
    debounceTime,
    distinctUntilChanged,
    concatMap,
    map,
    toArray,
    switchMap,
    takeUntil,
    finalize,
    of,
    catchError
} from 'rxjs';

import { AdminPlantService } from '../../../../core/services/admin-plant.service';
import {
    CategoryDto,
    PerenualSearchResult,
    PerenualPlantDetail,
    CreatePlantRequest
} from '../../../../core/models/plant.model';
import { resolvePlantImageUrl } from '../../../../core/utils/image-url.util';

type BulkImportItem = {
    id: number;
    name: string;
    scientificName: string;
    description?: string;
    imageUrl?: string;
    family?: string;
    genus?: string;
    price: number;
    categoryId: number;
    categoryName: string;
    selected: boolean;
    loading: boolean;
    error?: string;
};

@Component({
    selector: 'app-add-plant',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, FormlyModule, FormlyBootstrapModule, RouterLink],
    templateUrl: './add-plant.component.html',
    styleUrls: ['./add-plant.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddPlantComponent implements OnInit, OnDestroy {
    private destroy$ = new Subject<void>();
    private categories: CategoryDto[] = [];
    private currentSearchVersion = 0;
    readonly fallbackImageUrl = 'assets/images/plant-placeholder.svg';

    // ─── Form ─────────────────────────────────────────────────────────────────
    form = new FormGroup({});
    model: Record<string, unknown> = {};
    fields: FormlyFieldConfig[] = [];

    // ─── Search ───────────────────────────────────────────────────────────────
    searchControl = new FormControl<string>('');
    searchQuery$ = new Subject<string>();

    searchResults$ = new BehaviorSubject<PerenualSearchResult[]>([]);
    searching$ = new BehaviorSubject<boolean>(false);
    showDropdown = false;
    hasSearched = false;
    searchError: string | null = null;
    bulkItems$ = new BehaviorSubject<BulkImportItem[]>([]);

    // ─── Preview ─────────────────────────────────────────────────────────────
    previewImage: string | null = null;
    isEditMode = false;
    editingPlantId: number | null = null;

    // ─── Submit ────────────────────────────────────────────────────────────────
    submitting$ = new BehaviorSubject<boolean>(false);
    submitError: string | null = null;

    constructor(
        private plantService: AdminPlantService,
        private route: ActivatedRoute,
        private router: Router,
        private cdr: ChangeDetectorRef
    ) { }

    ngOnInit(): void {
        const idParam = this.route.snapshot.paramMap.get('id');
        if (idParam && !Number.isNaN(Number(idParam))) {
            this.isEditMode = true;
            this.editingPlantId = Number(idParam);
        }

        this.plantService.getCategories().pipe(takeUntil(this.destroy$)).subscribe(cats => {
            this.categories = cats;
            this.buildFields(cats);

            if (this.isEditMode && this.editingPlantId) {
                this.loadPlantForEdit(this.editingPlantId);
            }

            this.cdr.markForCheck();
        });

        // Perenual debounced search - tăng debounce để tránh rate limit
        this.searchQuery$.pipe(
            debounceTime(1000),
            distinctUntilChanged(),
            switchMap(query => {
                if (!query || query.trim().length < 3) {
                    this.searchResults$.next([]);
                    this.showDropdown = false;
                    this.hasSearched = false;
                    this.searchError = null;
                    this.cdr.markForCheck();
                    return of([]);
                }

                this.hasSearched = true;
                this.searching$.next(true);
                this.showDropdown = true;
                this.searchError = null;

                return this.plantService.searchPerenual(query.trim()).pipe(
                    catchError((err) => {
                        if (err.status === 429) {
                            this.searchError = 'Đã vượt quá giới hạn request (100/ngày). Vui lòng thử lại sau.';
                        } else if (err.status === 504) {
                            this.searchError = 'Perenual API phản hồi chậm. Vui lòng thử lại sau vài giây.';
                        } else {
                            this.searchError = 'Không thể tải dữ liệu từ Perenual. Kiểm tra API backend và thử lại.';
                        }
                        return of([]);
                    }),
                    finalize(() => {
                        this.searching$.next(false);
                        this.cdr.markForCheck();
                    })
                );
            }),
            takeUntil(this.destroy$)
        ).subscribe(results => {
            this.searchResults$.next(results);
            // Không hiển thị dropdown kết quả, chỉ cập nhật bulk list
            this.showDropdown = false;

            if (results.length > 0) {
                // Giới hạn 5 kết quả để tránh rate limit
                this.prepareBulkItemsFromSearch(results.slice(0, 20));
            } else {
                this.bulkItems$.next([]);
            }

            this.cdr.markForCheck();
        });
    }

    private loadPlantForEdit(id: number): void {
        this.searching$.next(true);
        this.plantService.getPlantById(id).pipe(
            finalize(() => {
                this.searching$.next(false);
                this.cdr.markForCheck();
            }),
            takeUntil(this.destroy$)
        ).subscribe({
            next: plant => {

                this.model = {
                    name: plant.name,
                    scientificName: plant.scientificName,
                    description: plant.description ?? '',
                    price: plant.price,
                    categoryId: plant.categoryId,
                    imageUrl: plant.imageUrl,
                    stockQuantity: plant.stockQuantity ?? null
                };
                this.previewImage = plant.imageUrl || null;
            },
            error: () => {
                this.submitError = 'Không tải được dữ liệu cây để chỉnh sửa.';
            }
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    // ─── Build Formly Fields ──────────────────────────────────────────────────
    private buildFields(categories: CategoryDto[]): void {
        const categoryOptions = categories.map(c => ({ value: c.id, label: c.name }));

        this.fields = [
            {
                fieldGroupClassName: 'row g-3',
                fieldGroup: [
                    {
                        key: 'name',
                        type: 'input',
                        className: 'col-md-6',
                        props: {
                            label: 'Tên cây',
                            placeholder: 'VD: Cây bàng',
                            required: true,
                            maxLength: 200
                        }
                    },
                    {
                        key: 'scientificName',
                        type: 'input',
                        className: 'col-md-6',
                        props: {
                            label: 'Tên khoa học',
                            placeholder: 'VD: Terminalia catappa',
                            maxLength: 200
                        }
                    },
                    {
                        key: 'description',
                        type: 'textarea',
                        className: 'col-12',
                        props: {
                            label: 'Mô tả',
                            placeholder: 'Nhập mô tả về cây...',
                            rows: 4
                        }
                    },
                    {
                        key: 'price',
                        type: 'input',
                        className: 'col-md-4',
                        props: {
                            label: 'Giá (VNĐ)',
                            type: 'number',
                            required: true,
                            min: 0.01,
                            step: 0.01,
                            placeholder: '0'
                        },
                        validators: {
                            positivePrice: {
                                expression: (control: { value: unknown }) => Number(control.value ?? 0) > 0,
                                message: 'Giá phải lớn hơn 0'
                            }
                        }
                    },
                    {
                        key: 'categoryId',
                        type: 'select',
                        className: 'col-md-4',
                        props: {
                            label: 'Danh mục',
                            required: true,
                            options: [{ value: null, label: '-- Chọn danh mục --' }, ...categoryOptions]
                        }
                    },
                    {
                        key: 'imageUrl',
                        type: 'input',
                        className: 'col-md-4',
                        props: {
                            label: 'URL Ảnh',
                            placeholder: 'https://...'
                        }
                    },
                    {
                        key: 'stockQuantity',
                        type: 'input',
                        className: 'col-md-4',
                        props: {
                            label: 'Tồn kho',
                            type: 'number',
                            min: 0,
                            placeholder: 'Số lượng trong kho'
                        }
                    }
                ]
            }
        ];
    }

    // ─── Perenual Handlers ────────────────────────────────────────────────────
    onSearchInput(event: Event): void {
        const value = (event.target as HTMLInputElement).value;
        this.searchQuery$.next(value);
    }

    onSelectPerenualResult(result: PerenualSearchResult): void {
        const name = result.commonName ?? result.scientificName;

        // Ẩn dropdown và clear input sau khi chọn
        this.showDropdown = false;
        this.hasSearched = false;
        this.searchControl.setValue('');
        this.searchResults$.next([]);
        this.searching$.next(true);

        const existingItem = this.bulkItems$.getValue().find(item => item.id === result.id);
        if (existingItem) {
            this.bulkItems$.next(this.bulkItems$.getValue().map(item =>
                item.id === result.id ? { ...item, selected: true } : item
            ));
            this.model = {
                ...this.model,
                name: existingItem.name,
                scientificName: existingItem.scientificName,
                description: existingItem.description ?? '',
                imageUrl: existingItem.imageUrl ?? '',
                price: existingItem.price,
                categoryId: existingItem.categoryId
            };
            this.previewImage = existingItem.imageUrl ?? null;
            this.searching$.next(false);
            this.cdr.markForCheck();
            return;
        }

        const randomCategory = this.pickRandomCategory();
        const newItem: BulkImportItem = {
            id: result.id,
            name,
            scientificName: result.scientificName,
            imageUrl: result.imageUrl ?? undefined,
            loading: true,
            price: this.randomPrice(),
            categoryId: randomCategory?.id ?? 0,
            categoryName: randomCategory?.name ?? 'Chưa có danh mục',
            selected: true
        };

        this.bulkItems$.next([...this.bulkItems$.getValue(), newItem]);

        this.plantService.getPerenualDetail(result.id).pipe(
            catchError(() => {
                // Nếu lỗi (429 hoặc khác), dùng thông tin từ search result
                return of({
                    id: result.id,
                    commonName: result.commonName,
                    scientificName: result.scientificName,
                    description: null,
                    imageUrl: result.imageUrl,
                    family: null,
                    genus: null
                } as PerenualPlantDetail);
            }),
            finalize(() => {
                this.searching$.next(false);
                this.cdr.markForCheck();
            }),
            takeUntil(this.destroy$)
        ).subscribe({
            next: (detail: PerenualPlantDetail) => {

                this.model = {
                    ...this.model,
                    name: detail.commonName ?? detail.scientificName,
                    scientificName: detail.scientificName,
                    description: detail.description ?? '',
                    imageUrl: detail.imageUrl ?? ''
                };

                this.previewImage = detail.imageUrl ?? null;
                this.bulkItems$.next(this.bulkItems$.getValue().map(item =>
                    item.id === detail.id
                        ? {
                            ...item,
                            name: detail.commonName ?? detail.scientificName,
                            scientificName: detail.scientificName,
                            description: detail.description ?? undefined,
                            family: detail.family ?? undefined,
                            genus: detail.genus ?? undefined,
                            imageUrl: detail.imageUrl ?? undefined,
                            loading: false
                        }
                        : item
                ));

                // Trigger formly update
                this.model = { ...this.model };
                this.cdr.markForCheck();
            }
        });
    }

    removeBulkItem(id: number): void {
        this.bulkItems$.next(this.bulkItems$.getValue().filter(item => item.id !== id));
        this.cdr.markForCheck();
    }

    clearBulkItems(): void {
        this.bulkItems$.next([]);
        this.cdr.markForCheck();
    }

    toggleBulkSelection(id: number, selected: boolean): void {
        this.bulkItems$.next(this.bulkItems$.getValue().map(item =>
            item.id === id ? { ...item, selected } : item
        ));
        this.cdr.markForCheck();
    }

    private prepareBulkItemsFromSearch(results: PerenualSearchResult[]): void {
        if (this.categories.length === 0) {
            this.searchError = 'Chưa tải được danh mục. Không thể auto gán dữ liệu import.';
            this.bulkItems$.next([]);
            return;
        }

        // Không gọi detail API để tránh rate limit (100 req/ngày)
        // Chỉ dùng thông tin từ search result
        const seedItems: BulkImportItem[] = results.map(result => {
            const randomCategory = this.pickRandomCategory();

            return {
                id: result.id,
                name: result.commonName ?? result.scientificName,
                scientificName: result.scientificName,
                description: undefined, // Không có description từ search
                imageUrl: result.imageUrl ?? undefined,
                price: this.randomPrice(),
                categoryId: randomCategory?.id ?? 0,
                categoryName: randomCategory?.name ?? 'Chưa có danh mục',
                selected: true,
                loading: false, // Không loading vì không gọi API
                error: undefined
            };
        });

        this.bulkItems$.next(seedItems);
        this.cdr.markForCheck();
    }

    private randomPrice(): number {
        const min = 120000;
        const max = 790000;
        const step = 1000;
        const randomStep = Math.floor(Math.random() * ((max - min) / step + 1));
        return min + randomStep * step;
    }

    private pickRandomCategory(): CategoryDto | undefined {
        if (this.categories.length === 0) {
            return undefined;
        }

        const index = Math.floor(Math.random() * this.categories.length);
        return this.categories[index];
    }

    onImageUrlChange(url: string): void {
        this.previewImage = url?.trim() || null;
        this.cdr.markForCheck();
    }

    /** Được gọi khi bất kỳ field nào trong formly thay đổi */
    onModelChange(model: Record<string, unknown>): void {
        const imageUrl = model['imageUrl'] as string | undefined;
        this.previewImage = imageUrl?.trim() || null;
        this.cdr.markForCheck();
    }

    closeDropdown(): void {
        setTimeout(() => {
            this.showDropdown = false;
            this.cdr.markForCheck();
        }, 200);
    }

    // ─── Submit ───────────────────────────────────────────────────────────────
    onSubmit(): void {
        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        this.submitError = null;
        this.submitting$.next(true);

        const payload: CreatePlantRequest = {
            name: String(this.model['name'] ?? ''),
            scientificName: String(this.model['scientificName'] ?? ''),
            description: this.model['description'] ? String(this.model['description']) : undefined,
            imageUrl: this.model['imageUrl'] ? String(this.model['imageUrl']) : undefined,
            price: Number(this.model['price'] ?? 0),
            categoryId: this.model['categoryId'] ? Number(this.model['categoryId']) : undefined,

            stockQuantity: this.model['stockQuantity'] !== undefined && this.model['stockQuantity'] !== '' 
                ? Number(this.model['stockQuantity']) 
                : null
        };

        const request$ = this.isEditMode && this.editingPlantId
            ? this.plantService.updatePlant(this.editingPlantId, payload)
            : this.plantService.createPlant(payload);

        request$.pipe(
            finalize(() => {
                this.submitting$.next(false);
                this.cdr.markForCheck();
            }),
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.router.navigate(['/admin/plants'], {
                    queryParams: this.isEditMode ? { updated: 1 } : { created: 1 }
                });
            },
            error: (err) => {
                this.submitError = err?.error?.message || err?.error?.title || 'Có lỗi xảy ra. Vui lòng thử lại.';
            }
        });
    }

    onBulkImport(): void {
        const items = this.bulkItems$.getValue().filter(item => item.selected);
        if (items.length === 0) {
            this.submitError = 'Không có cây nào được chọn để nhập hàng loạt.';
            this.cdr.markForCheck();
            return;
        }

        this.submitError = null;
        this.submitting$.next(true);

        from(items).pipe(
            concatMap(item => {
                const payload: CreatePlantRequest = {
                    name: item.name,
                    scientificName: item.scientificName,
                    description: item.description,
                    imageUrl: item.imageUrl,
                    price: item.price,
                    categoryId: item.categoryId,

                };

                return this.plantService.createPlant(payload).pipe(
                    map(() => ({ success: true, item })),
                    catchError(error => of({ success: false, item, error }))
                );
            }),
            toArray(),
            finalize(() => {
                this.submitting$.next(false);
                this.cdr.markForCheck();
            }),
            takeUntil(this.destroy$)
        ).subscribe(results => {
            const failed = results.filter(r => !r.success);

            if (failed.length > 0) {
                this.submitError = `Đã thêm thành công ${results.length - failed.length}/${results.length} cây. ${failed.length} cây bị lỗi.`;
                return;
            }

            this.router.navigate(['/admin/plants']);
        });
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
}

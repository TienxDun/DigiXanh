import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, shareReplay, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CategoryDto, PlantDetailDto, PlantDto } from '../models/plant.model';
import { PagedResult } from '../models/pagination.model';

export interface PublicPlantQuery {
  page: number;
  pageSize: number;
  search?: string;
  categoryId?: number | null;
  sortBy?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PublicPlantService {
  private apiUrl = `${environment.apiUrl}/plants`;
  private categoriesUrl = `${environment.apiUrl}/categories`;
  private readonly plantsCache = new Map<string, Observable<PagedResult<PlantDto>>>();
  private readonly detailCache = new Map<number, Observable<PlantDetailDto>>();
  private readonly relatedCache = new Map<string, Observable<PagedResult<PlantDto>>>();
  private readonly categories$ = this.http.get<CategoryDto[]>(this.categoriesUrl).pipe(
    shareReplay({ bufferSize: 1, refCount: false })
  );

  constructor(private http: HttpClient) { }

  getPlants(query: PublicPlantQuery): Observable<PagedResult<PlantDto>> {
    const cacheKey = this.getQueryCacheKey(query);
    const cached = this.plantsCache.get(cacheKey);

    if (cached) {
      return cached;
    }

    let params = new HttpParams()
      .set('page', query.page.toString())
      .set('pageSize', query.pageSize.toString());

    if (query.search) {
      params = params.set('search', query.search);
    }

    if (query.categoryId) {
      params = params.set('categoryId', query.categoryId.toString());
    }

    if (query.sortBy) {
      params = params.set('sortBy', query.sortBy);
    }

    const request$ = this.http.get<PagedResult<PlantDto>>(this.apiUrl, { params }).pipe(
      shareReplay({ bufferSize: 1, refCount: false }),
      catchError((error) => {
        this.plantsCache.delete(cacheKey);
        return throwError(() => error);
      })
    );

    this.setCacheItem(this.plantsCache, cacheKey, request$);
    return request$;
  }

  getCategories(): Observable<CategoryDto[]> {
    return this.categories$;
  }

  getPlantDetail(id: number): Observable<PlantDetailDto> {
    const cached = this.detailCache.get(id);

    if (cached) {
      return cached;
    }

    const request$ = this.http.get<PlantDetailDto>(`${this.apiUrl}/${id}`).pipe(
      shareReplay({ bufferSize: 1, refCount: false }),
      catchError((error) => {
        this.detailCache.delete(id);
        return throwError(() => error);
      })
    );

    this.setCacheItem(this.detailCache, id, request$);
    return request$;
  }

  /** Lấy sản phẩm liên quan cùng danh mục, loại trừ sản phẩm hiện tại */
  getRelatedPlants(categoryId: number, excludeId: number, pageSize = 4): Observable<PagedResult<PlantDto>> {
    const cacheKey = `${categoryId}-${excludeId}-${pageSize}`;
    const cached = this.relatedCache.get(cacheKey);

    if (cached) {
      return cached;
    }

    const params = new HttpParams()
      .set('page', '1')
      .set('pageSize', pageSize.toString())
      .set('categoryId', categoryId.toString());

    const request$ = this.http.get<PagedResult<PlantDto>>(this.apiUrl, { params }).pipe(
      shareReplay({ bufferSize: 1, refCount: false }),
      catchError((error) => {
        this.relatedCache.delete(cacheKey);
        return throwError(() => error);
      })
    );

    this.setCacheItem(this.relatedCache, cacheKey, request$);
    return request$;
  }

  private getQueryCacheKey(query: PublicPlantQuery): string {
    return JSON.stringify({
      page: query.page,
      pageSize: query.pageSize,
      search: query.search ?? '',
      categoryId: query.categoryId ?? null,
      sortBy: query.sortBy ?? ''
    });
  }

  private setCacheItem<K, V>(cache: Map<K, V>, key: K, value: V): void {
    // Keep cache bounded to avoid unbounded growth for highly variable queries.
    if (cache.size >= 100) {
      cache.clear();
    }

    cache.set(key, value);
  }
}

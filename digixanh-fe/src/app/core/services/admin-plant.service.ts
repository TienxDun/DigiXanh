import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { PagedResult } from '../models/pagination.model';
import {
  PlantDto,
  PlantDetailDto,
  CategoryDto,
  TrefleSearchResult,
  TreflePlantDetail,
  CreatePlantRequest
} from '../models/plant.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AdminPlantService {
  private readonly adminPlantsUrl = `${environment.apiUrl}/admin/plants`;
  private readonly categoriesUrl = `${environment.apiUrl}/categories`;
  private readonly adminTrefleUrl = `${environment.apiUrl}/admin/trefle`;
  private readonly legacyTrefleUrl = `${environment.apiUrl}/trefle`;

  constructor(private http: HttpClient) { }

  // ─── Plants CRUD ─────────────────────────────────────────────────────────

  getPlants(page: number, pageSize: number, search: string): Observable<PagedResult<PlantDto>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('search', search);
    }

    return this.http.get<PagedResult<PlantDto>>(this.adminPlantsUrl, { params });
  }

  createPlant(data: CreatePlantRequest): Observable<PlantDto> {
    return this.http.post<PlantDto>(this.adminPlantsUrl, data);
  }

  getPlantById(id: number): Observable<PlantDetailDto> {
    return this.http.get<PlantDetailDto>(`${this.adminPlantsUrl}/${id}`);
  }

  updatePlant(id: number, data: CreatePlantRequest): Observable<PlantDto> {
    return this.http.put<PlantDto>(`${this.adminPlantsUrl}/${id}`, data);
  }

  deletePlant(id: number): Observable<void> {
    return this.http.delete<void>(`${this.adminPlantsUrl}/${id}`);
  }

  deletePlantsBulk(ids: number[]): Observable<void> {
    return this.http.post<void>(`${this.adminPlantsUrl}/bulk-soft-delete`, { ids });
  }

  // ─── Categories ───────────────────────────────────────────────────────────

  getCategories(): Observable<CategoryDto[]> {
    return this.http.get<CategoryDto[]>(this.categoriesUrl);
  }

  // ─── Trefle Integration ───────────────────────────────────────────────────

  searchTrefle(query: string): Observable<TrefleSearchResult[]> {
    const params = new HttpParams().set('q', query);
    return this.http
      .get<unknown[]>(`${this.adminTrefleUrl}/search`, { params })
      .pipe(
        map(items => (items ?? []).map((item: unknown) => this.mapSearchItem(item))),
        catchError(() => this.http
          .get<{ data: unknown[] }>(`${this.legacyTrefleUrl}/search`, { params })
          .pipe(map(res => (res?.data ?? []).map((item: unknown) => this.mapSearchItem(item)))))
      );
  }

  getTrefleDetail(id: number): Observable<TreflePlantDetail> {
    return this.http
      .get<unknown>(`${this.adminTrefleUrl}/${id}`)
      .pipe(
        map(item => this.mapDetailItem(item)),
        catchError(() => this.http
          .get<{ data: unknown }>(`${this.legacyTrefleUrl}/plants/${id}`)
          .pipe(map(res => this.mapDetailItem(res?.data))))
      );
  }

  private mapSearchItem(input: unknown): TrefleSearchResult {
    const item = (input ?? {}) as Record<string, unknown>;
    const commonName = (item['commonName'] ?? item['common_name'] ?? null) as string | null;
    const scientificName = (item['scientificName'] ?? item['scientific_name'] ?? '') as string;
    const imageUrl = (item['imageUrl'] ?? item['image_url'] ?? null) as string | null;

    return {
      id: Number(item['id'] ?? 0),
      commonName,
      scientificName,
      imageUrl,
      common_name: commonName,
      scientific_name: scientificName,
      image_url: imageUrl
    };
  }

  private mapDetailItem(input: unknown): TreflePlantDetail {
    const item = (input ?? {}) as Record<string, unknown>;
    const commonName = (item['commonName'] ?? item['common_name'] ?? null) as string | null;
    const scientificName = (item['scientificName'] ?? item['scientific_name'] ?? '') as string;
    const imageUrl = (item['imageUrl'] ?? item['image_url'] ?? null) as string | null;

    return {
      id: Number(item['id'] ?? 0),
      commonName,
      scientificName,
      imageUrl,
      description: (item['description'] ?? null) as string | null,
      family: (item['family'] ?? null) as string | null,
      genus: (item['genus'] ?? null) as string | null,
      common_name: commonName,
      scientific_name: scientificName,
      image_url: imageUrl
    };
  }
}

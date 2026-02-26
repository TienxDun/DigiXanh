import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
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

  constructor(private http: HttpClient) { }

  getPlants(query: PublicPlantQuery): Observable<PagedResult<PlantDto>> {
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

    return this.http.get<PagedResult<PlantDto>>(this.apiUrl, { params });
  }

  getCategories(): Observable<CategoryDto[]> {
    return this.http.get<CategoryDto[]>(this.categoriesUrl);
  }

  getPlantDetail(id: number): Observable<PlantDetailDto> {
    return this.http.get<PlantDetailDto>(`${this.apiUrl}/${id}`);
  }
}

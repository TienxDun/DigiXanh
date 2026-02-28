import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CategoryDto } from '../models/plant.model';
import { environment } from '../../../environments/environment';

export interface CreateCategoryRequest {
  name: string;
  description?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminCategoryService {
  private readonly adminCategoriesUrl = `${environment.apiUrl}/admin/categories`;

  // Public Endpoint nếu cần
  private readonly publicCategoriesUrl = `${environment.apiUrl}/categories`;

  constructor(private http: HttpClient) { }

  // ─── Lấy danh sách danh mục (cho admin) ─────────────────────────
  getCategories(): Observable<CategoryDto[]> {
    return this.http.get<CategoryDto[]>(this.adminCategoriesUrl);
  }

  getCategoryById(id: number): Observable<CategoryDto> {
    return this.http.get<CategoryDto>(`${this.adminCategoriesUrl}/${id}`);
  }

  createCategory(data: CreateCategoryRequest): Observable<CategoryDto> {
    return this.http.post<CategoryDto>(this.adminCategoriesUrl, data);
  }

  updateCategory(id: number, data: CreateCategoryRequest): Observable<CategoryDto> {
    return this.http.put<CategoryDto>(`${this.adminCategoriesUrl}/${id}`, data);
  }

  deleteCategory(id: number): Observable<void> {
    return this.http.delete<void>(`${this.adminCategoriesUrl}/${id}`);
  }
}

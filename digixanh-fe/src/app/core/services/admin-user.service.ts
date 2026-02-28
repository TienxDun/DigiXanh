import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/pagination.model';
import {
  AdminUserDto,
  AdminUserDetailDto,
  UpdateUserLockoutRequest,
  UpdateUserRoleRequest,
  RoleDto
} from '../models/user-management.model';

@Injectable({
  providedIn: 'root'
})
export class AdminUserService {
  private readonly baseUrl = `${environment.apiUrl}/admin/users`;

  constructor(private http: HttpClient) { }

  /**
   * Lấy danh sách ngườI dùng với phân trang và tìm kiếm
   */
  getUsers(
    page: number,
    pageSize: number,
    search?: string,
    role?: string
  ): Observable<PagedResult<AdminUserDto>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('search', search);
    }

    if (role) {
      params = params.set('role', role);
    }

    return this.http.get<PagedResult<AdminUserDto>>(this.baseUrl, { params });
  }

  /**
   * Lấy chi tiết ngườI dùng
   */
  getUserById(id: string): Observable<AdminUserDetailDto> {
    return this.http.get<AdminUserDetailDto>(`${this.baseUrl}/${id}`);
  }

  /**
   * Khóa hoặc mở khóa tài khoản
   */
  updateUserLockout(id: string, request: UpdateUserLockoutRequest): Observable<{ message: string; userId: string; isLocked: boolean }> {
    return this.http.put<{ message: string; userId: string; isLocked: boolean }>(
      `${this.baseUrl}/${id}/lock`,
      request
    );
  }

  /**
   * Cập nhật role của ngườI dùng
   */
  updateUserRole(id: string, request: UpdateUserRoleRequest): Observable<{ message: string; userId: string; oldRoles: string[]; newRole: string }> {
    return this.http.put<{ message: string; userId: string; oldRoles: string[]; newRole: string }>(
      `${this.baseUrl}/${id}/role`,
      request
    );
  }

  /**
   * Lấy danh sách các roles
   */
  getRoles(): Observable<RoleDto[]> {
    return this.http.get<RoleDto[]>(`${this.baseUrl}/roles`);
  }

  /**
   * Kiểm tra xem user có phải là Admin không
   */
  isAdmin(user: AdminUserDto): boolean {
    return user.roles.some(r => r.toLowerCase() === 'admin');
  }

  /**
   * Kiểm tra xem user có phải là User không
   */
  isUser(user: AdminUserDto): boolean {
    return user.roles.some(r => r.toLowerCase() === 'user');
  }
}

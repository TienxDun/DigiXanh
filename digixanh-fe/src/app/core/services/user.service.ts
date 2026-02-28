import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UserProfile {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  address?: string;
  createdAt: Date;
}

export interface UpdateProfileRequest {
  fullName?: string;
  phoneNumber?: string;
  address?: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl = `${environment.apiUrl}/user`;

  constructor(private readonly http: HttpClient) {}

  /**
   * Lấy thông tin cá nhân (US21)
   */
  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/profile`);
  }

  /**
   * Cập nhật thông tin cá nhân (US21)
   */
  updateProfile(request: UpdateProfileRequest): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${this.apiUrl}/profile`, request);
  }
}

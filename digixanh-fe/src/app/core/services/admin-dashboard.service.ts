import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdminDashboardDto } from '../models/dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class AdminDashboardService {
  private readonly dashboardUrl = `${environment.apiUrl}/dashboard/admin`;

  constructor(private http: HttpClient) {}

  getAdminDashboard(): Observable<AdminDashboardDto> {
    return this.http.get<AdminDashboardDto>(this.dashboardUrl);
  }
}

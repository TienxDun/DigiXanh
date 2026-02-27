import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Router } from '@angular/router';

export interface LoginResponse {
  token: string;
  id: string;
  email: string;
  fullName: string;
  role: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
}

export interface RegisterResponse {
  message: string;
  userId?: string;
}

interface StoredUser {
  id: string;
  email: string;
  fullName: string;
  role: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private apiUrl = `${environment.apiUrl}/auth`;

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasToken());
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  private userSubject = new BehaviorSubject<StoredUser | null>(this.getStoredUser());
  public user$ = this.userSubject.asObservable();

  private hasToken(): boolean {
    return !!localStorage.getItem('token');
  }

  register(data: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.apiUrl}/register`, data);
  }

  isAuthenticated(): boolean {
    return this.hasToken();
  }

  isAdmin(): boolean {
    return this.hasRole('Admin');
  }

  hasRole(role: string): boolean {
    return this.getUserRole() === role;
  }

  getUserRole(): string | null {
    const storedUser = this.getStoredUser();
    if (storedUser?.role) {
      return storedUser.role;
    }

    const token = localStorage.getItem('token');
    if (!token) {
      return null;
    }

    const payload = this.decodeTokenPayload(token);
    const role = payload?.['role'] || payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

    return typeof role === 'string' ? role : null;
  }

  login(credentials: { email: string; password: string }): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap((response) => {
        if (response && response.token) {
          localStorage.setItem('token', response.token);

          const user: StoredUser = {
            id: response.id,
            email: response.email,
            fullName: response.fullName,
            role: response.role
          };
          localStorage.setItem('user', JSON.stringify(user));

          this.userSubject.next(user);
          this.isAuthenticatedSubject.next(true);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.userSubject.next(null);
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/']);
  }

  private getStoredUser(): StoredUser | null {
    const userRaw = localStorage.getItem('user');
    if (!userRaw) {
      return null;
    }

    try {
      return JSON.parse(userRaw) as StoredUser;
    } catch {
      return null;
    }
  }

  private decodeTokenPayload(token: string): Record<string, unknown> | null {
    const tokenParts = token.split('.');
    if (tokenParts.length < 2) {
      return null;
    }

    try {
      const base64 = tokenParts[1].replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map((char) => `%${`00${char.charCodeAt(0).toString(16)}`.slice(-2)}`)
          .join('')
      );

      return JSON.parse(jsonPayload) as Record<string, unknown>;
    } catch {
      return null;
    }
  }
}

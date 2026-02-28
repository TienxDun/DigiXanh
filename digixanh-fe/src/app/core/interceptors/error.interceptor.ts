import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // Token hết hạn hoặc không hợp lệ
        console.error('401 Unauthorized - Redirecting to login');
        authService.logout();
        router.navigate(['/auth/login'], {
          queryParams: { returnUrl: router.routerState.snapshot.url }
        });
      }

      if (error.status === 403) {
        // Không có quyền truy cập
        console.error('403 Forbidden');
        router.navigate(['/403']);
      }

      return throwError(() => error);
    })
  );
};

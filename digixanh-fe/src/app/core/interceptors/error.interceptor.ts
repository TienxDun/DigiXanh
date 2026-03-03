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
        console.error('401 Unauthorized - Checking context before redirect');

        const currentUrl = router.url;
        // Kiểm tra xem trang hiện tại có cần bảo vệ (admin, profile, orders, cart, checkout) hay không
        const isProtectedRoute =
          currentUrl.startsWith('/admin') ||
          currentUrl.startsWith('/profile') ||
          currentUrl.startsWith('/orders') ||
          currentUrl.startsWith('/cart') ||
          currentUrl.startsWith('/checkout');

        if (isProtectedRoute) {
          // Chỉ logout và về login nếu đang ở trang cần bảo mật
          authService.logout(false); // logout nhưng ko navigate đi đâu để interceptor tự xử lý
          router.navigate(['/auth/login'], {
            queryParams: { returnUrl: currentUrl }
          });
        } else {
          // Nếu ở trang công cộng (Home, Plants...), chỉ logout âm thầm để xóa trạng thái cũ
          // và cập nhật UI (vd: giỏ hàng về 0, hiện nút login)
          authService.logout(false);
          // Không navigate đi đâu cả
        }
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

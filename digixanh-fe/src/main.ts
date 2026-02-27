/// <reference types="@angular/localize" />
import { bootstrapApplication } from '@angular/platform-browser';

import { AppComponent } from './app/app.component';
import { appConfig } from './app/app.config';

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => {
    // Log error trong môi trường development
    if (typeof window !== 'undefined' && (window as unknown as { ngDevMode: boolean }).ngDevMode) {
      // eslint-disable-next-line no-console
      console.error('Application bootstrap failed:', err);
    }
    
    // Có thể gửi log về monitoring service (Sentry, LogRocket, etc.)
    // sendToErrorTracking(err);
    
    // Hiển thị thông báo lỗi cho user
    const errorElement = document.getElementById('bootstrap-error');
    if (errorElement) {
      errorElement.textContent = 'Không thể khởi động ứng dụng. Vui lòng tải lại trang.';
      errorElement.style.display = 'block';
    }
  });

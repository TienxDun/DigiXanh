import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient, withFetch } from '@angular/common/http';
import {
  provideRouter,
  withEnabledBlockingInitialNavigation,
  withHashLocation,
  withInMemoryScrolling,
  withRouterConfig,
  withViewTransitions
} from '@angular/router';
import { IconSetService } from '@coreui/icons-angular';
import { routes } from './app.routes';
import { FormlyModule } from '@ngx-formly/core';
import { FormlyBootstrapModule } from '@ngx-formly/bootstrap';
import { ReactiveFormsModule } from '@angular/forms';
import { ToastModule } from '@coreui/angular';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes,
      withRouterConfig({
        onSameUrlNavigation: 'reload'
      }),
      withInMemoryScrolling({
        scrollPositionRestoration: 'top',
        anchorScrolling: 'enabled'
      }),
      withEnabledBlockingInitialNavigation(),
      withViewTransitions(),
      withHashLocation()
    ),
    provideHttpClient(withFetch()),
    importProvidersFrom(
      ReactiveFormsModule,
      FormlyModule.forRoot({
        validationMessages: [
          { name: 'required', message: 'Trường này là bắt buộc' },
          { name: 'email', message: 'Email không hợp lệ' },
          { name: 'minLength', message: 'Mật khẩu phải có ít nhất 6 ký tự' },
          { name: 'passwordMatch', message: 'Mật khẩu xác nhận không khớp' },
          { name: 'serverError', message: (error: any) => error }
        ]
      }),
      FormlyBootstrapModule,
      ToastModule
    ),
    IconSetService,
    provideAnimationsAsync()
  ]
};


import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import {
  provideRouter,
  withEnabledBlockingInitialNavigation,
  withInMemoryScrolling,
  withRouterConfig
} from '@angular/router';
import { IconSetService } from '@coreui/icons-angular';
import { routes } from './app.routes';
import { FormlyModule } from '@ngx-formly/core';
import { FormlyBootstrapModule } from '@ngx-formly/bootstrap';
import { ReactiveFormsModule } from '@angular/forms';
import { ToastModule } from '@coreui/angular';
import { FormlyFieldPasswordComponent } from './core/forms/password-input.type';

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
      withEnabledBlockingInitialNavigation()
      // Note: withViewTransitions() removed - causing InvalidStateError
      // Note: withHashLocation() removed - using standard HTML5 history API
    ),
    provideHttpClient(withFetch(), withInterceptors([authInterceptor, errorInterceptor])),
    importProvidersFrom(
      ReactiveFormsModule,
      FormlyModule.forRoot({
        types: [
          { name: 'password', component: FormlyFieldPasswordComponent }
        ],
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


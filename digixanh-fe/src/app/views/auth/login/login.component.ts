import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { FormlyModule, FormlyFieldConfig } from '@ngx-formly/core';
import { FormlyBootstrapModule } from '@ngx-formly/bootstrap';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CardModule, FormModule, ButtonModule, SpinnerModule, ToastModule, ToasterComponent, ToastBodyComponent } from '@coreui/angular';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormlyModule,
    FormlyBootstrapModule,
    RouterModule,
    CardModule,
    FormModule,
    ButtonModule,
    SpinnerModule,
    ToastModule,
    ToasterComponent,
    ToastBodyComponent
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  form = new FormGroup({});
  model: any = {};
  isLoading = false;
  showToast = false;
  toastMessage = '';
  toastColor = 'success';
  errorMessage = '';

  fields: FormlyFieldConfig[] = [
    {
      key: 'email',
      type: 'input',
      props: {
        label: 'Email',
        placeholder: 'Nhập email',
        type: 'email',
        required: true,
      },
    },
    {
      key: 'password',
      type: 'password',
      props: {
        label: 'Mật khẩu',
        placeholder: 'Nhập mật khẩu',
        required: true,
      },
    },
  ];

  login() {
    if (this.form.valid) {
      this.isLoading = true;
      this.errorMessage = '';

      this.authService.login(this.model).subscribe({
        next: (res) => {
          this.isLoading = false;
          this.toastMessage = 'Đăng nhập thành công!';
          this.toastColor = 'success';
          this.showToast = true;

          const returnUrl = this.resolveReturnUrl();
          this.router.navigateByUrl(returnUrl).catch(() => {
            this.router.navigateByUrl('/');
          });
        },
        error: (err) => {
          this.isLoading = false;
          if (err.status === 401) {
            this.errorMessage = 'Email hoặc mật khẩu không đúng!';
          } else {
            this.toastMessage = err.error?.message || 'Đăng nhập thất bại. Vui lòng thử lại sau.';
            this.toastColor = 'danger';
            this.showToast = true;
          }
        },
      });
    }
  }

  private resolveReturnUrl(): string {
    const rawReturnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/';
    if (!rawReturnUrl.startsWith('/') || rawReturnUrl.startsWith('//')) {
      return '/';
    }

    if (rawReturnUrl === '/auth' || rawReturnUrl.startsWith('/auth/login')) {
      return '/';
    }

    return rawReturnUrl;
  }
}

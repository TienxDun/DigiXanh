import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule, AbstractControl } from '@angular/forms';
import { FormlyModule, FormlyFieldConfig } from '@ngx-formly/core';
import { FormlyBootstrapModule } from '@ngx-formly/bootstrap';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CardModule, FormModule, ButtonModule, SpinnerModule, ToastModule, ToasterComponent, ToastBodyComponent } from '@coreui/angular';

@Component({
  selector: 'app-register',
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
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  form = new FormGroup({});
  model: any = {};
  isLoading = false;
  showToast = false;
  toastMessage = '';
  toastColor = 'success';

  fields: FormlyFieldConfig[] = [
    {
      key: 'fullName',
      type: 'input',
      props: {
        label: 'Họ và tên',
        placeholder: 'Nhập họ và tên',
        required: true,
      },
    },
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
      type: 'input',
      props: {
        label: 'Mật khẩu',
        placeholder: 'Nhập mật khẩu',
        type: 'password',
        required: true,
        minLength: 6,
      },
    },
    {
      key: 'confirmPassword',
      type: 'input',
      props: {
        label: 'Xác nhận mật khẩu',
        placeholder: 'Nhập lại mật khẩu',
        type: 'password',
        required: true,
      },
      validators: {
        passwordMatch: {
          expression: (control: AbstractControl) => {
            return control.value === this.model.password;
          },
          message: 'Mật khẩu xác nhận không khớp',
        },
      },
    },
  ];

  register() {
    if (this.form.valid) {
      this.isLoading = true;
      const registerData = {
        fullName: this.model.fullName,
        email: this.model.email,
        password: this.model.password
      };

      this.authService.register(registerData).subscribe({
        next: (res) => {
          this.isLoading = false;
          this.toastMessage = 'Đăng ký thành công!';
          this.toastColor = 'success';
          this.showToast = true;
          setTimeout(() => {
            this.router.navigate(['/auth/login']);
          }, 1500);
        },
        error: (err) => {
          this.isLoading = false;
          if (err.error && err.error.errors) {
            Object.keys(err.error.errors).forEach(key => {
              const field = this.fields.find(f => f.key === key);
              if (field && field.formControl) {
                field.formControl.setErrors({ serverError: err.error.errors[key] });
                field.formControl.markAsTouched();
              }
            });
            this.toastMessage = 'Vui lòng kiểm tra lại thông tin!';
            this.toastColor = 'danger';
            this.showToast = true;
          } else {
            this.toastMessage = err.error?.message || 'Đăng ký thất bại. Vui lòng thử lại sau.';
            this.toastColor = 'danger';
            this.showToast = true;
          }
        },
      });
    }
  }
}


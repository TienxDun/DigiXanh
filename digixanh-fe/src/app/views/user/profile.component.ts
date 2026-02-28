import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { UserService, UserProfile, UpdateProfileRequest } from '../../core/services/user.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  profile: UserProfile | null = null;
  userRole: string = 'User';
  isLoading = false;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  // Form data
  fullName = '';
  phoneNumber = '';
  address = '';

  // Validation errors
  errors: { [key: string]: string } = {};

  ngOnInit(): void {
    this.loadProfile();
    this.loadUserRole();
  }

  loadUserRole(): void {
    const role = this.authService.getUserRole();
    this.userRole = role || 'User';
  }

  getRoleDisplayName(): string {
    switch (this.userRole) {
      case 'Admin':
        return 'Quản trị viên';
      case 'User':
        return 'Thành viên';
      default:
        return 'Thành viên';
    }
  }

  getRoleBadgeClass(): string {
    switch (this.userRole) {
      case 'Admin':
        return 'bg-danger bg-opacity-10 text-danger';
      case 'User':
        return 'bg-success bg-opacity-10 text-success';
      default:
        return 'bg-success bg-opacity-10 text-success';
    }
  }

  loadProfile(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.userService.getProfile().subscribe({
      next: (profile) => {
        this.profile = profile;
        this.fullName = profile.fullName || '';
        this.phoneNumber = profile.phoneNumber || '';
        this.address = profile.address || '';
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.errorMessage = 'Không thể tải thông tin cá nhân. Vui lòng thử lại sau.';
        this.isLoading = false;
        this.cdr.detectChanges();
        console.error('Error loading profile:', error);
      }
    });
  }

  validateForm(): boolean {
    this.errors = {};
    let isValid = true;

    // Validate FullName
    if (this.fullName.trim()) {
      if (this.fullName.trim().length < 2) {
        this.errors['fullName'] = 'Họ tên phải có ít nhất 2 ký tự.';
        isValid = false;
      } else if (this.fullName.trim().length > 200) {
        this.errors['fullName'] = 'Họ tên không được vượt quá 200 ký tự.';
        isValid = false;
      }
    }

    // Validate PhoneNumber (Vietnam)
    if (this.phoneNumber.trim()) {
      const phoneRegex = /^(0|84|\+84)(3|5|7|8|9)\d{8}$/;
      const normalizedPhone = this.phoneNumber.trim().startsWith('+')
        ? this.phoneNumber.trim().substring(1)
        : this.phoneNumber.trim();

      if (!phoneRegex.test(normalizedPhone)) {
        this.errors['phoneNumber'] = 'Số điện thoại không hợp lệ. Vui lòng nhập SĐT Việt Nam (VD: 0901234567).';
        isValid = false;
      }
    }

    // Validate Address
    if (this.address.trim() && this.address.trim().length > 500) {
      this.errors['address'] = 'Địa chỉ không được vượt quá 500 ký tự.';
      isValid = false;
    }

    return isValid;
  }

  saveProfile(): void {
    if (this.isSaving) return;

    if (!this.validateForm()) {
      this.cdr.detectChanges();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request: UpdateProfileRequest = {
      fullName: this.fullName.trim() || undefined,
      phoneNumber: this.phoneNumber.trim() || undefined,
      address: this.address.trim() || undefined
    };

    this.userService.updateProfile(request).subscribe({
      next: (updatedProfile) => {
        this.profile = updatedProfile;
        this.isSaving = false;
        this.successMessage = 'Cập nhật thông tin thành công!';

        // Cập nhật lại thông tin user trong auth service
        this.authService.user$.subscribe(currentUser => {
          if (currentUser) {
            currentUser.fullName = updatedProfile.fullName;
          }
        }).unsubscribe();

        this.cdr.detectChanges();

        // Ẩn thông báo thành công sau 3 giây
        setTimeout(() => {
          this.successMessage = '';
          this.cdr.detectChanges();
        }, 3000);
      },
      error: (error) => {
        this.isSaving = false;

        if (error.error?.errors) {
          // Xử lý lỗi validation từ server
          const serverErrors = error.error.errors;
          Object.keys(serverErrors).forEach(key => {
            this.errors[key] = serverErrors[key][0];
          });
        } else {
          this.errorMessage = error.error?.message || 'Có lỗi xảy ra khi cập nhật thông tin. Vui lòng thử lại sau.';
        }

        this.cdr.detectChanges();
        console.error('Error updating profile:', error);
      }
    });
  }

  goToOrders(): void {
    this.router.navigate(['/orders']);
  }

  formatDate(date: Date | string): string {
    return new Date(date).toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}

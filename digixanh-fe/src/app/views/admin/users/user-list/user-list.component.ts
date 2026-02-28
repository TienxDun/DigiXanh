import { Component, OnInit } from '@angular/core';
import { AsyncPipe, DatePipe, NgClass, NgFor, NgIf } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { BehaviorSubject, combineLatest, of } from 'rxjs';
import { catchError, finalize, switchMap, tap } from 'rxjs/operators';
import { AdminUserService } from '../../../../core/services/admin-user.service';
import { AdminUserDto, RoleDto } from '../../../../core/models/user-management.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-user-list',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, AsyncPipe, DatePipe, FormsModule],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit {
  users$ = new BehaviorSubject<AdminUserDto[]>([]);
  loading$ = new BehaviorSubject<boolean>(false);
  roles$ = new BehaviorSubject<RoleDto[]>([]);

  currentPage$ = new BehaviorSubject<number>(1);
  pageSize$ = new BehaviorSubject<number>(10);
  search$ = new BehaviorSubject<string>('');
  roleFilter$ = new BehaviorSubject<string>('');
  totalPages$ = new BehaviorSubject<number>(1);
  totalCount$ = new BehaviorSubject<number>(0);
  actionMessage$ = new BehaviorSubject<string | null>(null);

  Math = Math;

  // Role display names
  readonly roleDisplayNames: { [key: string]: string } = {
    'Admin': 'Quản trị viên',
    'User': 'NgườI dùng'
  };

  constructor(
    private adminUserService: AdminUserService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    // Load roles for filter dropdown
    this.adminUserService.getRoles().subscribe({
      next: (roles) => this.roles$.next(roles),
      error: () => this.roles$.next([])
    });

    // Handle messages from navigation
    this.route.queryParamMap.subscribe(params => {
      if (params.get('updated') === '1') {
        this.actionMessage$.next('Cập nhật thông tin ngườI dùng thành công.');
      } else if (params.get('locked') === '1') {
        this.actionMessage$.next('Đã khóa tài khoản thành công.');
      } else if (params.get('unlocked') === '1') {
        this.actionMessage$.next('Đã mở khóa tài khoản thành công.');
      } else if (params.get('roleChanged') === '1') {
        this.actionMessage$.next('Cập nhật phân quyền thành công.');
      }

      if (params.keys.length > 0) {
        this.router.navigate([], {
          relativeTo: this.route,
          queryParams: {},
          replaceUrl: true
        });
      }
    });

    // Load users when page, pageSize, search or role filter changes
    combineLatest([this.currentPage$, this.pageSize$, this.search$, this.roleFilter$]).pipe(
      tap(() => this.loading$.next(true)),
      switchMap(([page, pageSize, search, role]) =>
        this.adminUserService.getUsers(page, pageSize, search, role || undefined).pipe(
          tap((res) => {
            this.totalPages$.next(res.totalPages || 0);
            this.totalCount$.next(res.totalCount || 0);
            this.users$.next(res.items || []);
          }),
          catchError((err) => {
            console.error('Error loading users:', err);
            this.actionMessage$.next('Không thể tải danh sách ngườI dùng. Vui lòng thử lại.');
            return of({
              items: [],
              totalCount: 0,
              page: 1,
              pageSize,
              totalPages: 0
            });
          }),
          finalize(() => this.loading$.next(false))
        )
      )
    ).subscribe();
  }

  onSearch(term: string): void {
    this.search$.next(term);
    this.currentPage$.next(1);
  }

  onRoleChange(role: string): void {
    this.roleFilter$.next(role);
    this.currentPage$.next(1);
  }

  onPageChange(page: number): void {
    const currentTotalPages = this.totalPages$.getValue();
    if (page >= 1 && page <= currentTotalPages) {
      this.currentPage$.next(page);
    }
  }

  getPageArray(): number[] {
    const totalPages = this.totalPages$.getValue();
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }

  /**
   * Khóa hoặc mở khóa tài khoản
   */
  toggleUserLock(user: AdminUserDto): void {
    const action = user.isLocked ? 'mở khóa' : 'khóa';
    const confirmMessage = user.isLocked
      ? `Bạn có chắc chắn muốn mở khóa tài khoản của "${user.fullName}"?`
      : `Bạn có chắc chắn muốn khóa tài khoản của "${user.fullName}"?\n\nNgườI dùng sẽ không thể đăng nhập sau khi bị khóa.`;

    if (!confirm(confirmMessage)) {
      return;
    }

    this.loading$.next(true);
    this.adminUserService.updateUserLockout(user.id, {
      isLocked: !user.isLocked,
      reason: `Admin ${action} tài khoản`
    }).pipe(
      finalize(() => this.loading$.next(false))
    ).subscribe({
      next: (result) => {
        const message = result.isLocked
          ? 'Đã khóa tài khoản thành công.'
          : 'Đã mở khóa tài khoản thành công.';
        this.actionMessage$.next(message);
        // Reload current page
        this.currentPage$.next(this.currentPage$.getValue());
      },
      error: (err) => {
        console.error('Error updating user lockout:', err);
        this.actionMessage$.next('Cập nhật trạng thái thất bại. Vui lòng thử lại.');
      }
    });
  }

  /**
   * Đổi role của user
   */
  changeUserRole(user: AdminUserDto, newRole: string): void {
    const currentRole = this.getPrimaryRole(user);
    if (currentRole === newRole) {
      return;
    }

    const confirmMessage = `Bạn có chắc chắn muốn thay đổi phân quyền của "${user.fullName}" từ "${this.getRoleDisplayName(currentRole)}" thành "${this.getRoleDisplayName(newRole)}"?`;

    if (!confirm(confirmMessage)) {
      return;
    }

    this.loading$.next(true);
    this.adminUserService.updateUserRole(user.id, { role: newRole }).pipe(
      finalize(() => this.loading$.next(false))
    ).subscribe({
      next: () => {
        this.actionMessage$.next('Cập nhật phân quyền thành công.');
        // Reload current page
        this.currentPage$.next(this.currentPage$.getValue());
      },
      error: (err) => {
        console.error('Error updating user role:', err);
        this.actionMessage$.next('Cập nhật phân quyền thất bại. Vui lòng thử lại.');
      }
    });
  }

  /**
   * Lấy role chính của user (Admin > User)
   */
  getPrimaryRole(user: AdminUserDto): string {
    if (user.roles.some(r => r.toLowerCase() === 'admin')) {
      return 'Admin';
    }
    return 'User';
  }

  /**
   * Kiểm tra xem user có phải là Admin không
   */
  isAdmin(user: AdminUserDto): boolean {
    return this.adminUserService.isAdmin(user);
  }

  getRoleDisplayName(role: string): string {
    return this.roleDisplayNames[role] || role;
  }

  getRoleBadgeClass(user: AdminUserDto): string {
    return this.isAdmin(user)
      ? 'badge bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25'
      : 'badge bg-info bg-opacity-10 text-info border border-info border-opacity-25';
  }

  getLockBadgeClass(isLocked: boolean): string {
    return isLocked
      ? 'badge bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25'
      : 'badge bg-success bg-opacity-10 text-success border border-success border-opacity-25';
  }

  viewUserDetail(id: string): void {
    this.router.navigate(['/admin/users', id]);
  }

  dismissMessage(): void {
    this.actionMessage$.next(null);
  }
}

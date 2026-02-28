import { PagedResult } from './pagination.model';

export interface AdminUserDto {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  address?: string;
  createdAt: Date;
  lastLoginAt?: Date;
  isLocked: boolean;
  roles: string[];
}

export interface AdminUserDetailDto extends AdminUserDto {
  recentOrders: UserOrderSummaryDto[];
  totalOrders: number;
  totalSpent: number;
}

export interface UserOrderSummaryDto {
  id: number;
  orderDate: Date;
  finalAmount: number;
  status: string;
}

export interface UpdateUserLockoutRequest {
  isLocked: boolean;
  reason?: string;
}

export interface UpdateUserRoleRequest {
  role: string;
}

export interface RoleDto {
  name: string;
  displayName: string;
}

export type AdminUserListResponse = PagedResult<AdminUserDto>;

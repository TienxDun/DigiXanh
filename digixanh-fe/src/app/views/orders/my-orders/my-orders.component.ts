import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { OrderDto } from '../../../core/models/order.model';
import { PagedResult } from '../../../core/models/pagination.model';

interface StatusTab {
  key: string;       // '' = all, hoặc 'pending', 'paid', ...
  label: string;
  icon: string;
}

@Component({
  selector: 'app-my-orders',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './my-orders.component.html',
  styleUrls: ['./my-orders.component.scss']
})
export class MyOrdersComponent implements OnInit {
  private readonly orderService = inject(OrderService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  // ---- Data ----
  allOrders: OrderDto[] = [];
  orders: OrderDto[] = [];          // sau khi filter
  pagedResult: PagedResult<OrderDto> | null = null;
  currentPage = 1;
  readonly pageSize = 20;           // tải nhiều hơn để filter client-side đủ data

  // ---- UI State ----
  isLoading = false;
  errorMessage = '';
  activeStatus = '';                // '' = tất cả
  expandedOrders = new Set<number>();

  // ---- Tab definitions ----
  readonly statusTabs: StatusTab[] = [
    { key: '', label: 'Tất cả', icon: 'fa-solid fa-border-all' },
    { key: 'pending', label: 'Chờ xử lý', icon: 'fa-regular fa-clock' },
    { key: 'paid', label: 'Đã thanh toán', icon: 'fa-solid fa-circle-check' },
    { key: 'shipped', label: 'Đang giao', icon: 'fa-solid fa-truck' },
    { key: 'delivered', label: 'Đã giao', icon: 'fa-solid fa-box-open' },
    { key: 'cancelled', label: 'Đã hủy', icon: 'fa-solid fa-ban' },
  ];

  // ---- Status step flow ----
  readonly statusSteps = ['pending', 'paid', 'shipped', 'delivered'];

  ngOnInit(): void {
    this.loadOrders();
  }

  // ──────────────────────────────────────────────
  // Data loading
  // ──────────────────────────────────────────────
  loadOrders(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.orderService.getMyOrders({
      page: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: (result) => {
        this.pagedResult = result;
        this.allOrders = result.items;
        this.applyFilter();
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.errorMessage = 'Không thể tải danh sách đơn hàng. Vui lòng thử lại sau.';
        this.isLoading = false;
        console.error('Error loading orders:', error);
        this.cdr.markForCheck();
      }
    });
  }

  // ──────────────────────────────────────────────
  // Filter
  // ──────────────────────────────────────────────
  selectTab(statusKey: string): void {
    this.activeStatus = statusKey;
    this.applyFilter();
  }

  private applyFilter(): void {
    if (!this.activeStatus) {
      this.orders = this.allOrders;
    } else {
      this.orders = this.allOrders.filter(
        o => o.status.toLowerCase() === this.activeStatus
      );
    }
    this.cdr.markForCheck();
  }

  countByStatus(key: string): number {
    if (!key) return this.allOrders.length;
    return this.allOrders.filter(o => o.status.toLowerCase() === key).length;
  }

  // ──────────────────────────────────────────────
  // Expand / Collapse sản phẩm
  // ──────────────────────────────────────────────
  toggleExpand(orderId: number, event: Event): void {
    event.stopPropagation();
    if (this.expandedOrders.has(orderId)) {
      this.expandedOrders.delete(orderId);
    } else {
      this.expandedOrders.add(orderId);
    }
  }

  isExpanded(orderId: number): boolean {
    return this.expandedOrders.has(orderId);
  }

  // ──────────────────────────────────────────────
  // Pagination
  // ──────────────────────────────────────────────
  goToPage(page: number): void {
    if (page < 1 || (this.pagedResult && page > this.pagedResult.totalPages)) return;
    this.currentPage = page;
    this.loadOrders();
  }

  // ──────────────────────────────────────────────
  // Navigation
  // ──────────────────────────────────────────────
  viewOrderDetail(orderId: number): void {
    this.router.navigate(['/orders', orderId]);
  }

  // ──────────────────────────────────────────────
  // Status helpers
  // ──────────────────────────────────────────────
  getStatusDisplayName(status: string): string {
    const tab = this.statusTabs.find(t => t.key === status.toLowerCase());
    return tab?.label ?? status;
  }

  getStepIndex(status: string): number {
    return this.statusSteps.indexOf(status.toLowerCase());
  }

  isCancelled(status: string): boolean {
    return status.toLowerCase() === 'cancelled';
  }

  getStatusAccentColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending': return 'accent-warning';
      case 'paid': return 'accent-info';
      case 'shipped': return 'accent-primary';
      case 'delivered': return 'accent-success';
      case 'cancelled': return 'accent-danger';
      default: return 'accent-secondary';
    }
  }

  getStatusIcon(status: string): string {
    const tab = this.statusTabs.find(t => t.key === status.toLowerCase());
    return tab?.icon ?? 'fa-solid fa-circle';
  }

  getBootstrapStatusBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending': return 'badge-warning';
      case 'paid': return 'badge-info';
      case 'shipped': return 'badge-primary';
      case 'delivered': return 'badge-success';
      case 'cancelled': return 'badge-danger';
      default: return 'badge-secondary';
    }
  }

  // ──────────────────────────────────────────────
  // Payment helpers
  // ──────────────────────────────────────────────
  getPaymentMethodDisplay(method: string): string {
    switch (method.toLowerCase()) {
      case 'cash': return 'Tiền mặt';
      case 'vnpay': return 'VNPay';
      default: return method;
    }
  }

  getPaymentIcon(method: string): string {
    switch (method.toLowerCase()) {
      case 'cash': return 'fa-solid fa-money-bill-wave';
      case 'vnpay': return 'fa-solid fa-credit-card';
      default: return 'fa-solid fa-wallet';
    }
  }

  getPaymentBadgeClass(method: string): string {
    switch ((method || '').toLowerCase()) {
      case 'cash': return 'payment-cash';
      case 'vnpay': return 'payment-vnpay';
      default: return 'payment-default';
    }
  }

  // ──────────────────────────────────────────────
  // Formatters
  // ──────────────────────────────────────────────
  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency', currency: 'VND'
    }).format(amount);
  }

  formatDate(date: Date | string): string {
    const d = new Date(date);
    const time = d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    const dateStr = d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
    return `${time} · ${dateStr}`;
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = 'assets/images/plant-placeholder.svg';
  }
}

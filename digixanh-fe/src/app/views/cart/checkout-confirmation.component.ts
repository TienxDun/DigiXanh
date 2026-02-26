import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-checkout-confirmation',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="container py-4 py-lg-5">
      <div class="border rounded-4 bg-white shadow-sm p-4 p-lg-5 text-center">
        <h3 class="fw-bold mb-3">Xác nhận đơn hàng</h3>
        <p class="text-muted mb-4">Trang xác nhận đơn hàng chi tiết sẽ được hoàn thiện ở US11.</p>
        <a class="btn btn-outline-success" routerLink="/cart">Quay lại giỏ hàng</a>
      </div>
    </section>
  `
})
export class CheckoutConfirmationComponent {}

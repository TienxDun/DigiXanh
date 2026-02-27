import { Routes } from '@angular/router';
import { adminGuard } from './core/guards/admin.guard';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./layout').then((m) => m.PublicLayoutComponent),
    data: {
      title: 'Public'
    },
    children: [
      {
        path: '',
        loadChildren: () => import('./views/public/public.module').then((m) => m.PublicModule)
      },
      {
        path: 'cart',
        canActivate: [authGuard],
        loadComponent: () => import('./views/cart/cart.component').then((m) => m.CartComponent)
      },
      {
        path: 'checkout',
        canActivate: [authGuard],
        loadComponent: () => import('./views/cart/checkout.component').then((m) => m.CheckoutComponent)
      },
      {
        path: 'order-success',
        canActivate: [authGuard],
        loadComponent: () => import('./views/cart/order-success.component').then((m) => m.OrderSuccessComponent)
      },
      {
        path: 'payment-return',
        // Không cần authGuard vì VNPay redirect không mang theo token
        loadComponent: () => import('./views/cart/payment-return.component').then((m) => m.PaymentReturnComponent)
      }
    ]
  },
  {
    path: 'admin',
    loadComponent: () => import('./layout').then((m) => m.DefaultLayoutComponent),
    canActivate: [adminGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadChildren: () => import('./views/dashboard/routes').then((m) => m.routes)
      },
      {
        path: 'plants',
        loadChildren: () => import('./views/admin/admin.module').then(m => m.AdminModule),
        data: {
          title: 'Quản lý cây'
        }
      },
      {
        path: 'orders',
        loadComponent: () => import('./views/admin/orders/order-list/order-list.component').then(m => m.OrderListComponent),
        data: {
          title: 'Quản lý đơn hàng'
        }
      },
      {
        path: 'orders/:id',
        loadComponent: () => import('./views/admin/orders/order-detail/order-detail.component').then(m => m.OrderDetailComponent),
        data: {
          title: 'Chi tiết Đơn hàng'
        }
      }
    ]
  },
  {
    path: 'auth',
    loadChildren: () => import('./views/auth/auth.module').then((m) => m.AuthModule),
    data: {
      title: 'Auth'
    }
  },
  {
    path: '403',
    loadComponent: () => import('./views/pages/page403/page403.component').then(m => m.Page403Component),
    data: {
      title: 'Access Denied'
    }
  },
  { path: '**', redirectTo: '' }
];

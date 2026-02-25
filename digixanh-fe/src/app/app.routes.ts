import { Routes } from '@angular/router';
import { adminGuard } from './core/guards/admin.guard';

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

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
        loadComponent: () => import('./views/plants/plants.component').then((m) => m.PlantsComponent),
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
  { path: '**', redirectTo: '' }
];

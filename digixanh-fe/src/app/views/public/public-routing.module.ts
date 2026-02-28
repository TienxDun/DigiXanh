import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./homepage/homepage.component').then((m) => m.HomepageComponent),
    title: 'DigiXanh - Mang thiên nhiên về không gian sống'
  },
  {
    path: 'plants',
    loadComponent: () => import('./public-plant-list/public-plant-list.component').then((m) => m.PublicPlantListComponent),
    title: 'Danh sách cây cảnh - DigiXanh'
  },
  {
    path: 'plants/:id',
    loadComponent: () => import('./public-plant-detail/public-plant-detail.component').then((m) => m.PublicPlantDetailComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PublicRoutingModule {}
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: 'plants/:id',
    loadComponent: () => import('./public-plant-detail/public-plant-detail.component').then((m) => m.PublicPlantDetailComponent)
  },
  {
    path: '',
    loadComponent: () => import('./public-plant-list/public-plant-list.component').then((m) => m.PublicPlantListComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PublicRoutingModule {}
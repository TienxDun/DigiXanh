import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PlantListComponent } from './plants/plant-list/plant-list.component';

const routes: Routes = [
  {
    path: '',
    component: PlantListComponent,
    data: { title: 'Quản lý Cây xanh' }
  },
  {
    path: 'create',
    loadComponent: () =>
      import('./plants/add-plant/add-plant.component').then(m => m.AddPlantComponent),
    data: { title: 'Thêm cây mới' }
  },
  {
    path: 'edit/:id',
    loadComponent: () =>
      import('./plants/add-plant/add-plant.component').then(m => m.AddPlantComponent),
    data: { title: 'Chỉnh sửa cây' }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }

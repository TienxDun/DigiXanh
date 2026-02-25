import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { AdminRoutingModule } from './admin-routing.module';
import { PlantListComponent } from './plants/plant-list/plant-list.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    PlantListComponent,
    AdminRoutingModule
  ]
})
export class AdminModule { }

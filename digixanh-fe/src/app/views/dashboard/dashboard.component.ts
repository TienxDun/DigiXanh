import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  RowComponent,
  ButtonDirective
} from '@coreui/angular';

@Component({
  templateUrl: 'dashboard.component.html',
  styleUrls: ['dashboard.component.scss'],
  imports: [CardComponent, CardHeaderComponent, CardBodyComponent, RowComponent, ColComponent, ButtonDirective, RouterLink]
})
export class DashboardComponent {}

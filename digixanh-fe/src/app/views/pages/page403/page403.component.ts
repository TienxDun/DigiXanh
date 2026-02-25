import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  ButtonDirective,
  ColComponent,
  ContainerComponent,
  RowComponent
} from '@coreui/angular';

@Component({
  selector: 'app-page403',
  templateUrl: './page403.component.html',
  imports: [ContainerComponent, RowComponent, ColComponent, ButtonDirective, RouterLink]
})
export class Page403Component { }

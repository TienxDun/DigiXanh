import { Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SlicePipe, UpperCasePipe } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { ColorModeService } from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import {
  DropdownComponent,
  DropdownToggleDirective,
  DropdownMenuDirective,
  BreadcrumbRouterComponent,
  HeaderComponent,
  HeaderTogglerDirective,
  SidebarToggleDirective
} from '@coreui/angular';

@Component({
  selector: 'app-default-header',
  standalone: true,
  imports: [
    RouterLink,
    SlicePipe,
    UpperCasePipe,
    IconDirective,
    DropdownComponent,
    DropdownToggleDirective,
    DropdownMenuDirective,
    BreadcrumbRouterComponent,
    HeaderTogglerDirective,
    SidebarToggleDirective
  ],
  templateUrl: './default-header.component.html',
  styleUrls: ['./default-header.component.scss']
})
export class DefaultHeaderComponent extends HeaderComponent {
  sidebarId = input('sidebar1');

  private authService = inject(AuthService);
  private colorModeService = inject(ColorModeService);

  get currentUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }

  get isDark(): boolean {
    return this.colorModeService.colorMode() === 'dark';
  }

  toggleTheme(): void {
    const newMode = this.isDark ? 'light' : 'dark';
    this.colorModeService.colorMode.set(newMode);
  }

  logout(): void {
    this.authService.logout();
  }
}

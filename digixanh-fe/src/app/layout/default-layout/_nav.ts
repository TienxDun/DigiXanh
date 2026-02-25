import { INavData } from '@coreui/angular';

export const navItems: INavData[] = [
  {
    name: 'Dashboard',
    url: '/admin/dashboard',
    iconComponent: { name: 'cil-speedometer' }
  },
  {
    name: 'Quản lý cây',
    url: '/admin/plants',
    iconComponent: { name: 'cil-leaf' }
  }
];

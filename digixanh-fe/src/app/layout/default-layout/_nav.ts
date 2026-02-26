import { INavData } from '@coreui/angular';

export const navItems: INavData[] = [
  {
    title: true,
    name: 'Tổng quan'
  },
  {
    name: 'Dashboard',
    url: '/admin/dashboard',
    iconComponent: { name: 'cil-speedometer' }
  },
  {
    title: true,
    name: 'Danh mục quản lý'
  },
  {
    name: 'Quản lý cây',
    url: '/admin/plants',
    iconComponent: { name: 'cil-leaf' }
  }
];

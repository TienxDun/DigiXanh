import { INavData } from '@coreui/angular';

export const navItems: INavData[] = [
  {
    name: 'Tổng quan (Dashboard)',
    url: '/admin/dashboard',
    iconComponent: { name: 'cil-speedometer' }
  },
  {
    title: true,
    name: 'Vận hành & Bán hàng'
  },
  {
    name: 'Quản lý Đơn hàng',
    url: '/admin/orders',
    iconComponent: { name: 'cil-cart' }
  },
  {
    title: true,
    name: 'Quản lý Sản phẩm'
  },
  {
    name: 'Danh sách Cây cảnh',
    url: '/admin/plants',
    iconComponent: { name: 'cil-leaf' }
  },
  {
    name: 'Quản lý Danh mục',
    url: '/admin/categories',
    iconComponent: { name: 'cil-tags' }
  },
  {
    name: 'Thêm cây mới',
    url: '/admin/plants/create',
    iconComponent: { name: 'cil-plus' }
  }
  // Các menu dưới đây sẽ được thêm khi implement các features tương ứng:
  // {
  //   title: true,
  //   name: 'Khách hàng & Tương tác'
  // },
  // {
  //   name: 'Quản lý Khách hàng',
  //   url: '/admin/customers',
  //   iconComponent: { name: 'cil-user' }
  // },
  // {
  //   name: 'Đánh giá / Phản hồi',
  //   url: '/admin/reviews',
  //   iconComponent: { name: 'cil-star' }
  // },
  // {
  //   title: true,
  //   name: 'Hệ thống & Cài đặt'
  // },
  // {
  //   name: 'Phân quyền Admin',
  //   url: '/admin/accounts',
  //   iconComponent: { name: 'cil-shield-alt' }
  // },
  // {
  //   name: 'Cài đặt Cửa hàng',
  //   url: '/admin/settings',
  //   iconComponent: { name: 'cil-settings' }
  // }
];

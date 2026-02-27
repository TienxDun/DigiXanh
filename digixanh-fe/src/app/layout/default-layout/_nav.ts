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
    iconComponent: { name: 'cil-cart' },
    children: [
      {
        name: 'Tất cả Đơn hàng',
        url: '/admin/orders'
      },
      {
        name: 'Chờ xác nhận',
        url: '/admin/orders/pending'
      },
      {
        name: 'Đang giao & Hoàn tất',
        url: '/admin/orders/completed'
      }
    ]
  },
  {
    name: 'Giao dịch / Thanh toán',
    url: '/admin/transactions',
    iconComponent: { name: 'cil-money' }
  },
  {
    title: true,
    name: 'Quản lý Sản phẩm'
  },
  {
    name: 'Sản phẩm',
    url: '/admin/plants',
    iconComponent: { name: 'cil-leaf' },
    children: [
      {
        name: 'Danh sách Cây cảnh',
        url: '/admin/plants'
      },
      {
        name: 'Danh mục / Phân loại',
        url: '/admin/categories'
      }
    ]
  },
  {
    name: 'Kho hàng / Nhập xuất',
    url: '/admin/inventory',
    iconComponent: { name: 'cil-library' }
  },
  {
    name: 'Khuyến mãi / Voucher',
    url: '/admin/promotions',
    iconComponent: { name: 'cil-tags' }
  },
  {
    title: true,
    name: 'Khách hàng & Tương tác'
  },
  {
    name: 'Quản lý Khách hàng',
    url: '/admin/customers',
    iconComponent: { name: 'cil-user' }
  },
  {
    name: 'Đánh giá / Phản hồi',
    url: '/admin/reviews',
    iconComponent: { name: 'cil-star' }
  },
  {
    title: true,
    name: 'Hệ thống & Cài đặt'
  },
  {
    name: 'Phân quyền Admin',
    url: '/admin/accounts',
    iconComponent: { name: 'cil-shield-alt' }
  },
  {
    name: 'Cài đặt Cửa hàng',
    url: '/admin/settings',
    iconComponent: { name: 'cil-settings' }
  }
];

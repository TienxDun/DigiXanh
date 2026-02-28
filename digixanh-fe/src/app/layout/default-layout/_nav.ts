import { INavData } from '@coreui/angular';

export const navItems: INavData[] = [
  // ═══════════════════════════════════════════════════════════
  // 📊 TỔNG QUAN
  // ═══════════════════════════════════════════════════════════
  {
    name: 'Dashboard',
    url: '/admin/dashboard',
    iconComponent: { name: 'cil-speedometer' },
    badge: {
      color: 'info',
      text: 'LIVE'
    }
  },

  // ═══════════════════════════════════════════════════════════
  // 🛒 VẬN HÀNH & BÁN HÀNG
  // ═══════════════════════════════════════════════════════════
  {
    title: true,
    name: 'Vận hành',
    class: 'mt-2'
  },
  {
    name: 'Đơn hàng',
    url: '/admin/orders',
    iconComponent: { name: 'cil-basket' },
    badge: {
      color: 'success',
      text: 'Mới'
    }
  },

  // ═══════════════════════════════════════════════════════════
  // 🌿 SẢN PHẨM (Collapsible Group)
  // ═══════════════════════════════════════════════════════════
  {
    name: 'Sản phẩm',
    url: '/admin/plants',
    iconComponent: { name: 'cil-spreadsheet' },
    children: [
      {
        name: 'Danh sách Cây cảnh',
        url: '/admin/plants',
        iconComponent: { name: 'cil-leaf' }
      },
      {
        name: 'Thêm cây mới',
        url: '/admin/plants/create',
        iconComponent: { name: 'cil-plus' }
      },
      {
        name: 'Quản lý Danh mục',
        url: '/admin/categories',
        iconComponent: { name: 'cil-tags' }
      }
    ]
  },

  // ═══════════════════════════════════════════════════════════
  // 👥 KHÁCH HÀNG
  // ═══════════════════════════════════════════════════════════
  {
    title: true,
    name: 'Khách hàng',
    class: 'mt-2'
  },
  {
    name: 'NgườI dùng',
    url: '/admin/users',
    iconComponent: { name: 'cil-people' }
  },

  // ═══════════════════════════════════════════════════════════
  // ⚙️ HỆ THỐNG (Commented - sẵn sàng mở rộng)
  // ═══════════════════════════════════════════════════════════
  // {
  //   title: true,
  //   name: 'Hệ thống',
  //   class: 'mt-2'
  // },
  // {
  //   name: 'Đánh giá / Phản hồi',
  //   url: '/admin/reviews',
  //   iconComponent: { name: 'cil-star' }
  // },
  // {
  //   name: 'Phân quyền Admin',
  //   url: '/admin/accounts',
  //   iconComponent: { name: 'cil-shield-alt' }
  // },
  // {
  //   name: 'Cài đặt',
  //   url: '/admin/settings',
  //   iconComponent: { name: 'cil-settings' }
  // }
];

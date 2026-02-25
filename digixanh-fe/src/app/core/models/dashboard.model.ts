export interface DailyOrderStatDto {
  date: string;
  label: string;
  ordersCount: number;
}

export interface AdminDashboardDto {
  totalOrders: number;
  totalRevenue: number;
  todayOrders: number;
  todayRevenue: number;
  dailyOrders: DailyOrderStatDto[];
  generatedAt: string;
}

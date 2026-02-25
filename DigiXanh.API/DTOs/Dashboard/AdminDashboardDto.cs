namespace DigiXanh.API.DTOs.Dashboard;

public record AdminDashboardDto(
    int TotalOrders,
    decimal TotalRevenue,
    int TodayOrders,
    decimal TodayRevenue,
    IReadOnlyCollection<DailyOrderStatDto> DailyOrders,
    DateTime GeneratedAt);

public record DailyOrderStatDto(
    DateOnly Date,
    string Label,
    int OrdersCount);
namespace DigiXanh.API.DTOs.Orders;

/// <summary>
/// DTO cho lịch sử thay đổi trạng thái đơn hàng (Audit trail)
/// </summary>
public record OrderStatusHistoryDto(
    int Id,
    string OldStatus,
    string NewStatus,
    string? ChangedBy,
    string? Reason,
    DateTime ChangedAt);

using System.ComponentModel.DataAnnotations;

namespace DigiXanh.API.DTOs.Orders;

/// <summary>
/// Request để cập nhật trạng thái đơn hàng
/// </summary>
public class UpdateOrderStatusRequest
{
    /// <summary>
    /// Trạng thái mới (0: Pending, 1: Paid, 2: Shipped, 3: Delivered, 4: Cancelled)
    /// </summary>
    [Required(ErrorMessage = "Vui lòng chọn trạng thái mới")]
    [Range(0, 4, ErrorMessage = "Trạng thái không hợp lệ")]
    public int NewStatus { get; set; }

    /// <summary>
    /// Lý do thay đổi trạng thái (tùy chọn)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự")]
    public string? Reason { get; set; }
}

using DigiXanh.API.Models;

namespace DigiXanh.API.Services.Interfaces;

/// <summary>
/// Service gửi email xác nhận đơn hàng
/// </summary>
public interface IOrderEmailService
{
    /// <summary>
    /// Gửi email xác nhận đơn hàng sau khi đặt hàng thành công
    /// </summary>
    /// <param name="email">Địa chỉ email ngườI nhận</param>
    /// <param name="order">Thông tin đơn hàng</param>
    /// <param name="userFullName">Tên đầy đủ của ngườI dùng</param>
    /// <returns>Task</returns>
    Task SendOrderConfirmationEmailAsync(string email, Order order, string userFullName);

    /// <summary>
    /// Gửi email thông báo thanh toán thành công
    /// </summary>
    /// <param name="email">Địa chỉ email ngườI nhận</param>
    /// <param name="order">Thông tin đơn hàng</param>
    /// <param name="transactionId">Mã giao dịch</param>
    /// <returns>Task</returns>
    Task SendPaymentSuccessEmailAsync(string email, Order order, string transactionId);
}

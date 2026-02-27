using DigiXanh.API.Models;
using DigiXanh.API.Services.Interfaces;

namespace DigiXanh.API.Services.Implementations;

/// <summary>
/// Placeholder implementation cho OrderEmailService
/// Chỉ ghi log, không gửi email thực tế
/// </summary>
public class OrderEmailService : IOrderEmailService
{
    private readonly ILogger<OrderEmailService> _logger;

    public OrderEmailService(ILogger<OrderEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendOrderConfirmationEmailAsync(string email, Order order, string userFullName)
    {
        // Placeholder: Chỉ ghi log, không gửi email thực tế
        _logger.LogInformation(
            "[EMAIL-PLACEHOLDER] Gửi email xác nhận đơn hàng. " +
            "To: {Email}, OrderId: {OrderId}, Amount: {Amount:C}, " +
            "Recipient: {RecipientName}, Address: {Address}",
            email,
            order.Id,
            order.FinalAmount,
            order.RecipientName,
            order.ShippingAddress);

        // TODO: Triển khai gửi email thực tế khi có SMTP server
        // Ví dụ: Sử dụng SendGrid, AWS SES, hoặc SMTP server nội bộ

        return Task.CompletedTask;
    }

    public Task SendPaymentSuccessEmailAsync(string email, Order order, string transactionId)
    {
        // Placeholder: Chỉ ghi log, không gửi email thực tế
        _logger.LogInformation(
            "[EMAIL-PLACEHOLDER] Gửi email thanh toán thành công. " +
            "To: {Email}, OrderId: {OrderId}, TransactionId: {TransactionId}, Amount: {Amount:C}",
            email,
            order.Id,
            transactionId,
            order.FinalAmount);

        // TODO: Triển khai gửi email thực tế khi có SMTP server

        return Task.CompletedTask;
    }
}

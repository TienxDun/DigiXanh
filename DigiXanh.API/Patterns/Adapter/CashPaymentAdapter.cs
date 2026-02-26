using DigiXanh.API.Models;

namespace DigiXanh.API.Patterns.Adapter;

public class CashPaymentAdapter : IPaymentAdapter
{
    public Task<PaymentResult> ProcessPaymentAsync(Order order, PaymentInfo paymentInfo)
    {
        // Thanh toán tiền mặt: chỉ cập nhật trạng thái Pending
        order.Status = OrderStatus.Pending;
        order.PaymentMethod = PaymentMethod.Cash;
        
        return Task.FromResult(new PaymentResult
        {
            Success = true,
            Message = "Đơn hàng đã được đặt thành công. Bạn sẽ thanh toán khi nhận hàng.",
            TransactionId = null,
            PaymentUrl = null
        });
    }
}

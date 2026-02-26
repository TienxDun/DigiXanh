using DigiXanh.API.Data;
using DigiXanh.API.DTOs.Orders;
using DigiXanh.API.Models;
using DigiXanh.API.Patterns.Adapter;
using DigiXanh.API.Patterns.Decorator;
using Microsoft.EntityFrameworkCore;

namespace DigiXanh.API.Patterns.Facade;

public class OrderProcessingFacade
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPaymentAdapterFactory _paymentFactory;
    private readonly ILogger<OrderProcessingFacade> _logger;
    private readonly IConfiguration _config;

    public OrderProcessingFacade(
        ApplicationDbContext dbContext,
        IPaymentAdapterFactory paymentFactory,
        ILogger<OrderProcessingFacade> logger,
        IConfiguration config)
    {
        _dbContext = dbContext;
        _paymentFactory = paymentFactory;
        _logger = logger;
        _config = config;
    }

    public async Task<CreateOrderResponse> PlaceOrderAsync(
        string userId, 
        CreateOrderRequest request, 
        string ipAddress)
    {
        // 1. Validate - Kiểm tra giỏ hàng
        var cartItems = await _dbContext.CartItems
            .Include(ci => ci.Plant)
            .Where(ci => ci.UserId == userId && ci.Plant != null && !ci.Plant.IsDeleted)
            .ToListAsync();

        if (!cartItems.Any())
        {
            return new CreateOrderResponse
            {
                Success = false,
                Message = "Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi đặt hàng."
            };
        }

        // 2. Validate thông tin giao hàng
        if (string.IsNullOrWhiteSpace(request.RecipientName))
        {
            return new CreateOrderResponse { Success = false, Message = "Vui lòng nhập tên ngườI nhận." };
        }
        if (string.IsNullOrWhiteSpace(request.Phone))
        {
            return new CreateOrderResponse { Success = false, Message = "Vui lòng nhập số điện thoại." };
        }
        if (string.IsNullOrWhiteSpace(request.ShippingAddress))
        {
            return new CreateOrderResponse { Success = false, Message = "Vui lòng nhập địa chỉ giao hàng." };
        }

        // 3. Tính tổng tiền với Decorator pattern
        var priceCalculator = PriceCalculatorFactory.CreateCalculatorWithDiscounts();
        var (baseAmount, discountAmount, finalAmount) = priceCalculator.CalculatePriceWithDetails(cartItems);

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // 4. Tạo Order entity
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = baseAmount,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                Status = OrderStatus.Pending,
                RecipientName = request.RecipientName.Trim(),
                Phone = request.Phone.Trim(),
                ShippingAddress = request.ShippingAddress.Trim(),
                PaymentMethod = (PaymentMethod)request.PaymentMethod,
                OrderItems = cartItems.Select(ci => new OrderItem
                {
                    PlantId = ci.PlantId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Plant?.Price ?? 0
                }).ToList()
            };

            // 5. Lưu order để có Id cho thanh toán
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // 6. Xử lý thanh toán qua Adapter pattern
            var paymentAdapter = _paymentFactory.GetAdapter(order.PaymentMethod);
            var paymentInfo = new PaymentInfo
            {
                IpAddress = ipAddress,
                ReturnUrl = request.ReturnUrl
            };
            var paymentResult = await paymentAdapter.ProcessPaymentAsync(order, paymentInfo);

            if (!paymentResult.Success)
            {
                await transaction.RollbackAsync();
                return new CreateOrderResponse
                {
                    Success = false,
                    Message = paymentResult.Message
                };
            }

            // 7. Cập nhật transactionId nếu có
            if (!string.IsNullOrEmpty(paymentResult.TransactionId))
            {
                order.TransactionId = paymentResult.TransactionId;
            }

            // 8. Lưu lại order sau khi xử lý thanh toán
            await _dbContext.SaveChangesAsync();

            // 9. Xóa giỏ hàng
            _dbContext.CartItems.RemoveRange(cartItems);
            await _dbContext.SaveChangesAsync();

            // 10. Commit transaction
            await transaction.CommitAsync();

            // 11. Trả về response
            return new CreateOrderResponse
            {
                Success = true,
                OrderId = order.Id,
                Message = paymentResult.Message,
                PaymentUrl = paymentResult.PaymentUrl,
                Order = MapToOrderDetailDto(order)
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing order for user {UserId}", userId);
            return new CreateOrderResponse
            {
                Success = false,
                Message = "Có lỗi xảy ra khi xử lý đơn hàng. Vui lòng thử lại sau."
            };
        }
    }

    public async Task<VNPayReturnResponse> ProcessVNPayReturnAsync(Dictionary<string, string> vnpayData)
    {
        var vnp_HashSecret = _config["VNPay:HashSecret"] ?? "";
        var vnp_SecureHash = vnpayData.GetValueOrDefault("vnp_SecureHash");
        
        _logger.LogInformation("Processing VNPay return with data: {Data}", 
            string.Join(", ", vnpayData.Select(x => $"{x.Key}={x.Value}")));
        
        // Validate signature
        var isValidSignature = VnPayLibrary.ValidateSignature(vnpayData, vnp_SecureHash ?? "", vnp_HashSecret);

        if (!isValidSignature)
        {
            _logger.LogWarning("Invalid VNPay signature");
            return new VNPayReturnResponse 
            { 
                Success = false, 
                Message = "Chữ ký không hợp lệ.",
                Status = "InvalidSignature"
            };
        }

        if (!int.TryParse(vnpayData.GetValueOrDefault("vnp_TxnRef"), out var orderId) || orderId <= 0)
        {
            return new VNPayReturnResponse
            {
                Success = false,
                Message = "Mã đơn hàng không hợp lệ.",
                Status = "InvalidOrderId"
            };
        }

        var vnp_ResponseCode = vnpayData.GetValueOrDefault("vnp_ResponseCode");
        var vnp_TransactionStatus = vnpayData.GetValueOrDefault("vnp_TransactionStatus");
        var vnp_TransactionNo = vnpayData.GetValueOrDefault("vnp_TransactionNo");

        _logger.LogInformation(
            "VNPay return - OrderId: {OrderId}, ResponseCode: {ResponseCode}, TransactionStatus: {TransactionStatus}, TransactionNo: {TransactionNo}",
            orderId,
            vnp_ResponseCode,
            vnp_TransactionStatus,
            vnp_TransactionNo);

        var order = await _dbContext.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Plant)
            .FirstOrDefaultAsync(o => o.Id == orderId);
            
        if (order == null)
        {
            return new VNPayReturnResponse { Success = false, Message = "Không tìm thấy đơn hàng.", Status = "OrderNotFound" };
        }

        // Theo tài liệu VNPay: giao dịch thành công khi ResponseCode = 00 và TransactionStatus = 00
        var isSuccess = vnp_ResponseCode == "00" && vnp_TransactionStatus == "00";

        if (isSuccess)
        {
            // TC08: Thanh toán thành công
            order.Status = OrderStatus.Paid;
            order.TransactionId = vnp_TransactionNo;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} paid successfully via VNPay", orderId);

            return new VNPayReturnResponse
            {
                Success = true,
                OrderId = order.Id,
                Status = "Paid",
                Message = "Thanh toán thành công.",
                TransactionId = vnp_TransactionNo,
                Order = MapToOrderDetailDto(order)
            };
        }
        else
        {
            // TC09: Thanh toán thất bại hoặc bị hủy
            order.Status = OrderStatus.Cancelled;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Order {OrderId} payment failed or cancelled. ResponseCode: {ResponseCode}, TransactionStatus: {TransactionStatus}",
                orderId,
                vnp_ResponseCode,
                vnp_TransactionStatus);

            var message = vnp_ResponseCode switch
            {
                "24" => "Đã hủy thanh toán.",
                "25" => "Giao dịch không tìm thấy.",
                "51" => "Tài khoản không đủ số dư.",
                "65" => "Tài khoản đã vượt quá hạn mức giao dịch.",
                "75" => "Ngân hàng thanh toán đang bảo trì.",
                "99" => "Lỗi không xác định từ ngân hàng.",
                _ => GetTransactionStatusMessage(vnp_TransactionStatus)
                    ?? $"Thanh toán thất bại. Mã lỗi: {vnp_ResponseCode ?? "N/A"}"
            };

            var status = vnp_ResponseCode == "24" ? "Cancelled" : "Failed";

            return new VNPayReturnResponse
            {
                Success = false,
                OrderId = order.Id,
                Status = status,
                Message = message,
                Order = MapToOrderDetailDto(order)
            };
        }
    }

    private static string? GetTransactionStatusMessage(string? transactionStatus)
    {
        return transactionStatus switch
        {
            "01" => "Giao dịch chưa hoàn tất.",
            "02" => "Giao dịch bị lỗi.",
            "04" => "Giao dịch đảo.",
            "05" => "VNPay đang xử lý giao dịch.",
            "06" => "VNPay đã gửi yêu cầu hoàn tiền sang ngân hàng.",
            "07" => "Giao dịch bị nghi ngờ gian lận.",
            "09" => "Giao dịch hoàn trả bị từ chối.",
            _ => null
        };
    }

    private static OrderDetailDto MapToOrderDetailDto(Order order)
    {
        return new OrderDetailDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            Status = order.Status.ToString(),
            PaymentMethod = order.PaymentMethod.ToString(),
            TransactionId = order.TransactionId,
            RecipientName = order.RecipientName,
            Phone = order.Phone,
            ShippingAddress = order.ShippingAddress,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                PlantId = oi.PlantId,
                PlantName = oi.Plant?.Name ?? $"Sản phẩm #{oi.PlantId}",
                ScientificName = oi.Plant?.ScientificName,
                ImageUrl = oi.Plant?.ImageUrl,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                LineTotal = oi.LineTotal
            }).ToList()
        };
    }
}

using DigiXanh.API.Data;
using DigiXanh.API.DTOs.Orders;
using DigiXanh.API.Models;
using DigiXanh.API.Patterns.Adapter;
using DigiXanh.API.Patterns.Decorator;
using DigiXanh.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigiXanh.API.Patterns.Facade;

/// <summary>
/// Facade Pattern - Đóng gói toàn bộ quy trình xử lý đơn hàng
/// Bao gồm: Validate → Tính giá → Tạo Order → Thanh toán → Email → Xóa giỏ hàng
/// </summary>
public class OrderProcessingFacade
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPaymentAdapterFactory _paymentFactory;
    private readonly IOrderEmailService _emailService;
    private readonly ILogger<OrderProcessingFacade> _logger;
    private readonly IConfiguration _config;

    public OrderProcessingFacade(
        ApplicationDbContext dbContext,
        IPaymentAdapterFactory paymentFactory,
        IOrderEmailService emailService,
        ILogger<OrderProcessingFacade> logger,
        IConfiguration config)
    {
        _dbContext = dbContext;
        _paymentFactory = paymentFactory;
        _emailService = emailService;
        _logger = logger;
        _config = config;
    }

    /// <summary>
    /// Quy trình đặt hàng hoàn chỉnh:
    /// 1. Validate dữ liệu đầu vào
    /// 2. Tính tổng tiền với decorator giảm giá
    /// 3. Tạo Order và OrderItem
    /// 4. Xử lý thanh toán qua Adapter
    /// 5. Gửi email xác nhận
    /// 6. Xóa giỏ hàng
    /// 7. Commit transaction
    /// </summary>
    public async Task<CreateOrderResponse> PlaceOrderAsync(
        string userId, 
        CreateOrderRequest request, 
        string ipAddress)
    {
        // ==================== STEP 1: Validate dữ liệu ====================
        var validationResult = await ValidateOrderDataAsync(userId, request);
        if (!validationResult.IsValid)
        {
            return new CreateOrderResponse
            {
                Success = false,
                Message = validationResult.ErrorMessage!
            };
        }

        var cartItems = validationResult.CartItems!;
        var user = validationResult.User!;

        // ==================== STEP 2: Tính tổng tiền với Decorator Pattern ====================
        var includeTax = _config.GetValue<bool?>("Pricing:EnableTax") ?? false;
        var taxPercent = _config.GetValue<decimal?>("Pricing:TaxPercent") ?? 0m;
        var includeShipping = _config.GetValue<bool?>("Pricing:EnableShipping") ?? false;
        var shippingFee = _config.GetValue<decimal?>("Pricing:ShippingFee") ?? 0m;

        var priceCalculator = PriceCalculatorFactory.CreateCalculatorWithDiscounts(
            includeTax: includeTax,
            taxPercent: taxPercent,
            includeShipping: includeShipping,
            shippingFee: shippingFee);
        var (baseAmount, discountAmount, finalAmount) = priceCalculator.CalculatePriceWithDetails(cartItems);

        _logger.LogInformation(
            "[Order] Price calculation - UserId: {UserId}, Base: {Base:C}, Discount: {Discount:C}, Final: {Final:C}",
            userId, baseAmount, discountAmount, finalAmount);

        // ==================== STEP 3-7: Transaction ====================
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // 3. Tạo Order entity
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

            // Lưu order để có Id cho thanh toán
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("[Order] Created OrderId: {OrderId} for UserId: {UserId}", order.Id, userId);

            // 4. Xử lý thanh toán qua Adapter Pattern
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
                _logger.LogWarning("[Order] Payment failed for OrderId: {OrderId}, Reason: {Reason}", 
                    order.Id, paymentResult.Message);
                return new CreateOrderResponse
                {
                    Success = false,
                    Message = paymentResult.Message
                };
            }

            // Cập nhật transactionId nếu có
            if (!string.IsNullOrEmpty(paymentResult.TransactionId))
            {
                order.TransactionId = paymentResult.TransactionId;
            }

            if (!string.IsNullOrWhiteSpace(paymentResult.PaymentUrl))
            {
                order.PaymentUrl = paymentResult.PaymentUrl;
            }

            // Lưu lại order sau khi xử lý thanh toán
            await _dbContext.SaveChangesAsync();

            // 5. GIẢM TỒN KHO (US26) - Chỉ giảm ngay với Cash. VNPay sẽ giảm khi callback thành công
            if (order.PaymentMethod == PaymentMethod.Cash)
            {
                await DeductStockAsync(order.OrderItems);
            }

            // 6. Gửi email xác nhận (placeholder - không ảnh hưởng transaction)
            try
            {
                await _emailService.SendOrderConfirmationEmailAsync(
                    user.Email ?? string.Empty, 
                    order, 
                    user.FullName ?? user.UserName ?? "Khách hàng");
            }
            catch (Exception ex)
            {
                // Email lỗi không rollback transaction
                _logger.LogWarning(ex, "[Order] Failed to send confirmation email for OrderId: {OrderId}", order.Id);
            }

            // 7. Xóa giỏ hàng
            _dbContext.CartItems.RemoveRange(cartItems);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("[Order] Cleared cart for UserId: {UserId}, {ItemCount} items removed", 
                userId, cartItems.Count);

            // 7. Commit transaction
            await transaction.CommitAsync();

            _logger.LogInformation(
                "[Order] Successfully completed OrderId: {OrderId}, PaymentMethod: {PaymentMethod}",
                order.Id, order.PaymentMethod);

            // Trả về response
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
            _logger.LogError(ex, "[Order] Error processing order for user {UserId}", userId);
            return new CreateOrderResponse
            {
                Success = false,
                Message = "Có lỗi xảy ra khi xử lý đơn hàng. Vui lòng thử lại sau."
            };
        }
    }

    /// <summary>
    /// Validate toàn bộ dữ liệu đặt hàng
    /// </summary>
    private async Task<ValidationResult> ValidateOrderDataAsync(string userId, CreateOrderRequest request)
    {
        // Validate user tồn tại
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return ValidationResult.Failure("Không tìm thấy thông tin ngườI dùng.");
        }

        // Validate giỏ hàng không trống
        var cartItems = await _dbContext.CartItems
            .Include(ci => ci.Plant)
            .Where(ci => ci.UserId == userId && ci.Plant != null && !ci.Plant.IsDeleted)
            .ToListAsync();

        if (!cartItems.Any())
        {
            return ValidationResult.Failure("Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi đặt hàng.");
        }

        // Validate số lượng sản phẩm trong kho
        foreach (var item in cartItems)
        {
            if (item.Quantity <= 0)
            {
                return ValidationResult.Failure($"Số lượng sản phẩm '{item.Plant?.Name}' không hợp lệ.");
            }

            // Kiểm tra tồn kho (US26)
            if (item.Plant?.StockQuantity.HasValue == true)
            {
                if (item.Quantity > item.Plant.StockQuantity.Value)
                {
                    return ValidationResult.Failure(
                        $"Sản phẩm '{item.Plant.Name}' chỉ còn {item.Plant.StockQuantity.Value} trong kho. " +
                        $"Vui lòng giảm số lượng hoặc chọn sản phẩm khác.");
                }
            }
        }

        // Validate thông tin giao hàng
        if (string.IsNullOrWhiteSpace(request.RecipientName))
        {
            return ValidationResult.Failure("Vui lòng nhập tên ngườI nhận.");
        }

        if (request.RecipientName.Length < 2 || request.RecipientName.Length > 200)
        {
            return ValidationResult.Failure("Tên ngườI nhận phải từ 2 đến 200 ký tự.");
        }

        if (string.IsNullOrWhiteSpace(request.Phone))
        {
            return ValidationResult.Failure("Vui lòng nhập số điện thoại.");
        }

        // Validate số điện thoại VN
        var phoneRegex = new System.Text.RegularExpressions.Regex(@"^(0[0-9]{9,10})$|^\+84[0-9]{9,10}$");
        if (!phoneRegex.IsMatch(request.Phone.Trim()))
        {
            return ValidationResult.Failure("Số điện thoại không hợp lệ. Vui lòng nhập số điện thoại Việt Nam (10-11 số).");
        }

        if (string.IsNullOrWhiteSpace(request.ShippingAddress))
        {
            return ValidationResult.Failure("Vui lòng nhập địa chỉ giao hàng.");
        }

        if (request.ShippingAddress.Length < 10 || request.ShippingAddress.Length > 500)
        {
            return ValidationResult.Failure("Địa chỉ giao hàng phải từ 10 đến 500 ký tự.");
        }

        // Validate phương thức thanh toán
        if (!Enum.IsDefined(typeof(PaymentMethod), (int)request.PaymentMethod))
        {
            return ValidationResult.Failure("Phương thức thanh toán không hợp lệ.");
        }

        if (request.PaymentMethod == (int)PaymentMethod.VNPay)
        {
            var configuredReturnUrl = _config["VNPay:ReturnUrl"];
            var configuredFrontendReturnUrl = _config["VNPay:FrontendReturnUrl"];
            var hasClientReturnUrl = IsValidAbsoluteHttpUrl(request.ReturnUrl);
            var hasConfiguredReturnUrl = IsValidAbsoluteHttpUrl(configuredReturnUrl);
            var hasConfiguredFrontendReturnUrl = IsValidAbsoluteHttpUrl(configuredFrontendReturnUrl);

            if (!hasClientReturnUrl && !hasConfiguredReturnUrl && !hasConfiguredFrontendReturnUrl)
            {
                return ValidationResult.Failure("Cấu hình callback VNPay không hợp lệ. Vui lòng cấu hình ReturnUrl/FrontendReturnUrl là URL tuyệt đối (http/https).");
            }

            if (!hasClientReturnUrl)
            {
                _logger.LogWarning("[Order] VNPay payment requested without client ReturnUrl for UserId: {UserId}. Fallback configuredReturnUrl={ConfiguredReturnUrl}, configuredFrontendReturnUrl={ConfiguredFrontendReturnUrl}",
                    userId,
                    configuredReturnUrl,
                    configuredFrontendReturnUrl);
            }
        }

        return ValidationResult.Success(cartItems, user);
    }

    /// <summary>
    /// Giảm tồn kho sau khi thanh toán thành công (US26)
    /// </summary>
    private async Task DeductStockAsync(IEnumerable<OrderItem> orderItems)
    {
        var plantIds = orderItems.Select(oi => oi.PlantId).Distinct().ToList();
        
        // Load plants với tracking để cập nhật
        var plants = await _dbContext.Plants
            .Where(p => plantIds.Contains(p.Id) && p.StockQuantity.HasValue)
            .ToDictionaryAsync(p => p.Id);

        foreach (var orderItem in orderItems)
        {
            if (plants.TryGetValue(orderItem.PlantId, out var plant))
            {
                var oldStock = plant.StockQuantity!.Value;
                var newStock = Math.Max(0, oldStock - orderItem.Quantity);
                plant.StockQuantity = newStock;
                
                _logger.LogInformation(
                    "[Stock] Deducted {Quantity} from PlantId: {PlantId}. Old: {OldStock}, New: {NewStock}",
                    orderItem.Quantity, orderItem.PlantId, oldStock, newStock);
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Xử lý VNPay Return URL - cập nhật trạng thái đơn hàng sau thanh toán
    /// </summary>
    public async Task<VNPayReturnResponse> ProcessVNPayReturnAsync(Dictionary<string, string> vnpayData)
    {
        var vnp_HashSecret = _config["VNPay:HashSecret"] ?? "";
        var vnp_SecureHash = vnpayData.GetValueOrDefault("vnp_SecureHash");
        
        _logger.LogInformation("[VNPay-Return] Processing with data count: {Count}", vnpayData.Count);
        _logger.LogInformation("[VNPay-Return] SecureHash from VNPay: {SecureHash}", vnp_SecureHash);
        _logger.LogInformation("[VNPay-Return] HashSecret configured: {HasHashSecret}", !string.IsNullOrEmpty(vnp_HashSecret));
        
        // Validate signature
        var isValidSignature = VnPayLibrary.ValidateSignature(
            new Dictionary<string, string>(vnpayData, StringComparer.OrdinalIgnoreCase), 
            vnp_SecureHash ?? "", 
            vnp_HashSecret);

        if (!isValidSignature)
        {
            _logger.LogWarning("[VNPay-Return] Invalid VNPay signature. Check HashSecret configuration.");
            return new VNPayReturnResponse 
            { 
                Success = false, 
                Message = "Chữ ký không hợp lệ. Vui lòng kiểm tra cấu hình HashSecret.",
                Status = "InvalidSignature"
            };
        }
        
        _logger.LogInformation("[VNPay-Return] Signature validated successfully");

        if (!int.TryParse(vnpayData.GetValueOrDefault("vnp_TxnRef"), out var orderId) || orderId <= 0)
        {
            _logger.LogWarning("[VNPay-Return] Invalid order id from callback. TxnRef={TxnRef}", vnpayData.GetValueOrDefault("vnp_TxnRef"));
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
        var vnp_Amount = vnpayData.GetValueOrDefault("vnp_Amount");

        _logger.LogInformation(
            "VNPay return - OrderId: {OrderId}, ResponseCode: {ResponseCode}, TransactionStatus: {TransactionStatus}, TransactionNo: {TransactionNo}",
            orderId,
            vnp_ResponseCode,
            vnp_TransactionStatus,
            vnp_TransactionNo);

        var order = await _dbContext.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Plant)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);
            
        if (order == null)
        {
            _logger.LogWarning("[VNPay-Return] Order not found. OrderId={OrderId}", orderId);
            return new VNPayReturnResponse { Success = false, Message = "Không tìm thấy đơn hàng.", Status = "OrderNotFound" };
        }

        if (!long.TryParse(vnp_Amount, out var amountInVnPayUnit))
        {
            _logger.LogWarning("[VNPay-Return] Invalid amount format. OrderId={OrderId}, AmountRaw={AmountRaw}", order.Id, vnp_Amount);
            return new VNPayReturnResponse
            {
                Success = false,
                OrderId = order.Id,
                Status = "InvalidAmount",
                Message = "Số tiền giao dịch không hợp lệ."
            };
        }

        var callbackAmount = amountInVnPayUnit / 100m;
        if (callbackAmount != order.FinalAmount)
        {
            _logger.LogWarning("[VNPay-Return] Amount mismatch. OrderId={OrderId}, Expected={Expected}, Received={Received}",
                order.Id,
                order.FinalAmount,
                callbackAmount);
            return new VNPayReturnResponse
            {
                Success = false,
                OrderId = order.Id,
                Status = "InvalidAmount",
                Message = "Số tiền giao dịch không khớp với đơn hàng."
            };
        }

        var isSuccess = vnp_ResponseCode == "00" && vnp_TransactionStatus == "00";

        if (isSuccess)
        {
            if (order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.Paid;
                if (!string.IsNullOrWhiteSpace(vnp_TransactionNo))
                {
                    order.TransactionId = vnp_TransactionNo;
                }

                await _dbContext.SaveChangesAsync();

                // GIẢM TỒN KHO (US26) - Sau khi VNPay thanh toán thành công
                // Load OrderItems nếu chưa có
                if (!order.OrderItems.Any())
                {
                    await _dbContext.Entry(order)
                        .Collection(o => o.OrderItems)
                        .LoadAsync();
                }
                await DeductStockAsync(order.OrderItems.ToList());

                // Gửi email thanh toán thành công
                try
                {
                    if (order.User != null)
                    {
                        await _emailService.SendPaymentSuccessEmailAsync(
                            order.User.Email ?? string.Empty,
                            order,
                            vnp_TransactionNo ?? "N/A");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[VNPay-Return] Failed to send payment success email for OrderId: {OrderId}", order.Id);
                }

                _logger.LogInformation("[VNPay-Return] Order marked as Paid. OrderId={OrderId}, TransactionNo={TransactionNo}", order.Id, vnp_TransactionNo);
            }
            else
            {
                _logger.LogInformation("[VNPay-Return] Order already processed. OrderId={OrderId}, CurrentStatus={CurrentStatus}", order.Id, order.Status);
            }

            return new VNPayReturnResponse
            {
                Success = true,
                OrderId = order.Id,
                Status = order.Status.ToString(),
                Message = "Thanh toán thành công.",
                TransactionId = vnp_TransactionNo,
                Order = MapToOrderDetailDto(order)
            };
        }
        else
        {
            if (order.Status == OrderStatus.Pending)
            {
                // === GIỮ LẠI ĐƠN HÀNG và KHÔI PHỤC GIỎ HÀNG khi thanh toán thất bại ===
                _logger.LogInformation("[VNPay-Return] Payment failed/cancelled. Marking OrderId={OrderId} as Cancelled. ResponseCode={ResponseCode}", order.Id, vnp_ResponseCode);

                await RestoreCartFromOrderAsync(order);

                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedBy = "VNPay";

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("[VNPay-Return] Order status updated to Cancelled successfully. OrderId={OrderId}", order.Id);
            }
            else
            {
                _logger.LogInformation("[VNPay-Return] Failure callback received but order already processed. OrderId={OrderId}, CurrentStatus={CurrentStatus}", order.Id, order.Status);
            }

            var message = vnp_ResponseCode switch
            {
                "24" => "Đã hủy thanh toán. Giỏ hàng của bạn đã được khôi phục.",
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

    private async Task RestoreCartFromOrderAsync(Order order)
    {
        if (!order.OrderItems.Any())
        {
            await _dbContext.Entry(order)
                .Collection(o => o.OrderItems)
                .LoadAsync();
        }

        var plantIds = order.OrderItems.Select(oi => oi.PlantId).Distinct().ToList();
        var existingCartItems = await _dbContext.CartItems
            .Where(c => c.UserId == order.UserId && plantIds.Contains(c.PlantId))
            .ToListAsync();

        foreach (var orderItem in order.OrderItems)
        {
            var existing = existingCartItems.FirstOrDefault(c => c.PlantId == orderItem.PlantId);
            if (existing != null)
            {
                existing.Quantity += orderItem.Quantity;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _dbContext.CartItems.Add(new CartItem
                {
                    UserId = order.UserId,
                    PlantId = orderItem.PlantId,
                    Quantity = orderItem.Quantity,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        _logger.LogInformation("[VNPay-Return] Restored {Count} items to cart for UserId={UserId}", order.OrderItems.Count, order.UserId);
    }

    /// <summary>
    /// Xử lý VNPay IPN - Instant Payment Notification
    /// </summary>
    public async Task<VNPayIpnResponse> ProcessVNPayIpnAsync(Dictionary<string, string> vnpayData)
    {
        try
        {
            var vnp_HashSecret = _config["VNPay:HashSecret"] ?? "";
            var vnp_SecureHash = vnpayData.GetValueOrDefault("vnp_SecureHash");

            var isValidSignature = VnPayLibrary.ValidateSignature(new Dictionary<string, string>(vnpayData), vnp_SecureHash ?? "", vnp_HashSecret);
            if (!isValidSignature)
            {
                return new VNPayIpnResponse { RspCode = "97", Message = "Invalid signature" };
            }

            if (!int.TryParse(vnpayData.GetValueOrDefault("vnp_TxnRef"), out var orderId) || orderId <= 0)
            {
                _logger.LogWarning("[VNPay-IPN] Invalid order id. TxnRef={TxnRef}", vnpayData.GetValueOrDefault("vnp_TxnRef"));
                return new VNPayIpnResponse { RspCode = "01", Message = "Order not found" };
            }

            var order = await _dbContext.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
                
            if (order == null)
            {
                _logger.LogWarning("[VNPay-IPN] Order not found. OrderId={OrderId}", orderId);
                return new VNPayIpnResponse { RspCode = "01", Message = "Order not found" };
            }

            var amountRaw = vnpayData.GetValueOrDefault("vnp_Amount");
            if (!long.TryParse(amountRaw, out var amountInVnPayUnit))
            {
                return new VNPayIpnResponse { RspCode = "04", Message = "Invalid amount" };
            }

            var amount = amountInVnPayUnit / 100m;
            if (amount != order.FinalAmount)
            {
                _logger.LogWarning("[VNPay-IPN] Amount mismatch. OrderId={OrderId}, Expected={Expected}, Received={Received}", order.Id, order.FinalAmount, amount);
                return new VNPayIpnResponse { RspCode = "04", Message = "Invalid amount" };
            }

            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogInformation("[VNPay-IPN] Order already confirmed. OrderId={OrderId}, CurrentStatus={CurrentStatus}", order.Id, order.Status);
                return new VNPayIpnResponse { RspCode = "02", Message = "Order already confirmed" };
            }

            var vnp_ResponseCode = vnpayData.GetValueOrDefault("vnp_ResponseCode");
            var vnp_TransactionStatus = vnpayData.GetValueOrDefault("vnp_TransactionStatus");
            var vnp_TransactionNo = vnpayData.GetValueOrDefault("vnp_TransactionNo");

            var isSuccess = vnp_ResponseCode == "00" && vnp_TransactionStatus == "00";
            if (isSuccess)
            {
                // Chỉ giảm stock nếu order đang Pending (tránh giảm 2 lần nếu IPN và Return cùng gọi)
                var shouldDeductStock = order.Status == OrderStatus.Pending;
                
                order.Status = OrderStatus.Paid;
                order.TransactionId = vnp_TransactionNo;
                _logger.LogInformation("[VNPay-IPN] Payment success. OrderId={OrderId}, TransactionNo={TransactionNo}, DeductStock={Deduct}", order.Id, vnp_TransactionNo, shouldDeductStock);

                // GIẢM TỒN KHO (US26) - Chỉ giảm một lần
                if (shouldDeductStock)
                {
                    await _dbContext.Entry(order)
                        .Collection(o => o.OrderItems)
                        .LoadAsync();
                    await DeductStockAsync(order.OrderItems.ToList());
                }

                // Gửi email thanh toán thành công
                try
                {
                    if (order.User != null)
                    {
                        await _emailService.SendPaymentSuccessEmailAsync(
                            order.User.Email ?? string.Empty,
                            order,
                            vnp_TransactionNo ?? "N/A");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[VNPay-IPN] Failed to send payment success email for OrderId: {OrderId}", order.Id);
                }
            }
            else
            {
                order.Status = OrderStatus.Cancelled;
                _logger.LogInformation("[VNPay-IPN] Payment failed/cancelled. OrderId={OrderId}, ResponseCode={ResponseCode}, TransactionStatus={TransactionStatus}", order.Id, vnp_ResponseCode, vnp_TransactionStatus);
            }

            await _dbContext.SaveChangesAsync();
            return new VNPayIpnResponse { RspCode = "00", Message = "Confirm Success" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VNPay IPN");
            return new VNPayIpnResponse { RspCode = "99", Message = "Unknown error" };
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

    private static bool IsValidAbsoluteHttpUrl(string? url)
    {
        return Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri)
               && (uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                   || uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));
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

/// <summary>
/// Kết quả validation đặt hàng
/// </summary>
internal class ValidationResult
{
    public bool IsValid { get; private init; }
    public string? ErrorMessage { get; private init; }
    public List<CartItem>? CartItems { get; private init; }
    public ApplicationUser? User { get; private init; }

    public static ValidationResult Success(List<CartItem> cartItems, ApplicationUser user)
    {
        return new ValidationResult
        {
            IsValid = true,
            CartItems = cartItems,
            User = user
        };
    }

    public static ValidationResult Failure(string errorMessage)
    {
        return new ValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
}

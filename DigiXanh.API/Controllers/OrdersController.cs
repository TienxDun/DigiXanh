using System.Security.Claims;
using DigiXanh.API.DTOs.Orders;
using DigiXanh.API.Models;
using DigiXanh.API.Patterns.Facade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderProcessingFacade _orderFacade;
    private readonly ILogger<OrdersController> _logger;
    private readonly IConfiguration _configuration;

    public OrdersController(
        OrderProcessingFacade orderFacade,
        ILogger<OrdersController> logger,
        IConfiguration configuration)
    {
        _orderFacade = orderFacade;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Dữ liệu đặt hàng không hợp lệ." });
        }

        _logger.LogInformation("Received order request: RecipientName={RecipientName}, Phone={Phone}, Address={Address}, PaymentMethod={PaymentMethod}", 
            request?.RecipientName, request?.Phone, request?.ShippingAddress, request?.PaymentMethod);
        
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            _logger.LogWarning("Model validation failed: {Errors}", string.Join(", ", errors));
            return BadRequest(new { message = "Dữ liệu không hợp lệ", errors });
        }
        
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Không xác định được ngườI dùng." });
        }

        var ipAddress = GetIpAddress();
        request.ReturnUrl ??= BuildPaymentReturnUrl();
        var result = await _orderFacade.PlaceOrderAsync(userId, request, ipAddress);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result);
    }

    // NOTE: Endpoint này giữ lại để tương thích, nhưng chính thức sử dụng /api/payment/vnpay-return
    // Xem PaymentController.VNPayReturn() cho xử lý đầy đủ
    [HttpGet("payment-return")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(VNPayReturnResponse), StatusCodes.Status200OK)]
    [Obsolete("Sử dụng /api/payment/vnpay-return thay thế")]
    public async Task<IActionResult> VNPayReturn()
    {
        // Lấy dữ liệu gốc (raw) để validate signature chính xác
        var vnpayData = ParseQueryStringRaw(Request.QueryString.Value);

        var result = await _orderFacade.ProcessVNPayReturnAsync(vnpayData);

        if (WantsJsonResponse())
        {
            return Ok(result);
        }

        var frontendReturnUrl = _configuration["VNPay:FrontendReturnUrl"]
                                ?? "http://localhost:4200/payment-return";

        // Thêm thông tin kết quả thanh toán vào query string
        var redirectQuery = Request.Query.ToDictionary(x => x.Key, x => (string?)x.Value.ToString());
        redirectQuery["paymentStatus"] = result.Success ? "success" : 
            (result.Status?.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) == true ? "cancelled" : "failed");
        redirectQuery["message"] = result.Message;
        redirectQuery["orderId"] = result.OrderId > 0 ? result.OrderId.ToString() : null;
        redirectQuery["transactionId"] = result.TransactionId;

        var redirectUrl = QueryHelpers.AddQueryString(frontendReturnUrl, redirectQuery);
        return Redirect(redirectUrl);
    }

    [HttpGet("payment-ipn")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(VNPayIpnResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> VNPayIpn()
    {
        // Lấy dữ liệu gốc (raw) để validate signature chính xác
        var vnpayData = ParseQueryStringRaw(Request.QueryString.Value);

        var result = await _orderFacade.ProcessVNPayIpnAsync(vnpayData);
        return Ok(result);
    }

    /// <summary>
    /// Parse query string và giữ nguyên giá trị URL-encoded (không decode)
    /// </summary>
    private static Dictionary<string, string> ParseQueryStringRaw(string? queryString)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        if (string.IsNullOrWhiteSpace(queryString))
        {
            return result;
        }

        var query = queryString.TrimStart('?');
        var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            var key = parts[0];
            var value = parts.Length > 1 ? parts[1] : string.Empty;
            result[key] = value;
        }

        return result;
    }

    private bool WantsJsonResponse()
    {
        var acceptHeader = Request.Headers.Accept.ToString();
        return acceptHeader.Contains("application/json", StringComparison.OrdinalIgnoreCase)
               || Request.Query.ContainsKey("format");
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue(ClaimTypes.Name)
               ?? User.FindFirstValue("sub");
    }

    private string GetIpAddress()
    {
        var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ip))
        {
            ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        }
        return ip.Split(',')[0].Trim();
    }

    private string BuildPaymentReturnUrl()
    {
        return $"{Request.Scheme}://{Request.Host}/api/payment/vnpay-return";
    }
}

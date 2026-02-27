using DigiXanh.API.DTOs.Orders;
using DigiXanh.API.Helpers;
using DigiXanh.API.Patterns.Facade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : BaseController
{
    private readonly OrderProcessingFacade _orderFacade;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        OrderProcessingFacade orderFacade,
        ILogger<OrdersController> logger)
    {
        _orderFacade = orderFacade;
        _logger = logger;
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
            request.RecipientName, request.Phone, request.ShippingAddress, request.PaymentMethod);
        
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
        var result = await _orderFacade.PlaceOrderAsync(userId, request, ipAddress);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result);
    }

    [HttpGet("payment-ipn")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(VNPayIpnResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> VNPayIpn()
    {
        var vnpayData = QueryStringHelper.ParseRaw(Request.QueryString.Value);
        var result = await _orderFacade.ProcessVNPayIpnAsync(vnpayData);
        return Ok(result);
    }
}

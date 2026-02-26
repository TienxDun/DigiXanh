using System.Security.Claims;
using DigiXanh.API.DTOs.Orders;
using DigiXanh.API.Models;
using DigiXanh.API.Patterns.Facade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderProcessingFacade _orderFacade;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(OrderProcessingFacade orderFacade, ILogger<OrdersController> logger)
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
        var result = await _orderFacade.PlaceOrderAsync(userId, request, ipAddress);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result);
    }

    [HttpGet("payment-return")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(VNPayReturnResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> VNPayReturn()
    {
        var vnpayData = Request.Query
            .ToDictionary(x => x.Key, x => x.Value.ToString());

        var result = await _orderFacade.ProcessVNPayReturnAsync(vnpayData);
        return Ok(result);
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
}

using DigiXanh.API.Data;
using DigiXanh.API.DTOs.Common;
using DigiXanh.API.DTOs.Orders;
using DigiXanh.API.Helpers;
using DigiXanh.API.Models;
using DigiXanh.API.Patterns.Facade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : BaseController
{
    private readonly OrderProcessingFacade _orderFacade;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        OrderProcessingFacade orderFacade,
        ApplicationDbContext dbContext,
        ILogger<OrdersController> logger)
    {
        _orderFacade = orderFacade;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách đơn hàng của ngườI dùng hiện tại (US18)
    /// </summary>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        pageSize = pageSize > 50 ? 50 : pageSize; // Giới hạn tối đa 50 items/page

        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Không xác định được ngườI dùng." });
        }

        var query = _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId);

        var totalCount = await query.CountAsync();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var orders = await query
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Plant)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                DiscountAmount = o.DiscountAmount,
                FinalAmount = o.FinalAmount,
                Status = o.Status.ToString(),
                PaymentMethod = o.PaymentMethod.ToString(),
                TransactionId = o.TransactionId,
                Items = o.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    PlantId = oi.PlantId,
                    PlantName = oi.Plant != null ? oi.Plant.Name : "Không xác định",
                    ScientificName = oi.Plant != null ? oi.Plant.ScientificName : null,
                    ImageUrl = oi.Plant != null ? ImageUrlSanitizer.NormalizeOrEmpty(oi.Plant.ImageUrl) : null,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    LineTotal = oi.LineTotal
                }).ToList()
            })
            .ToListAsync();

        return Ok(new PagedResult<OrderDto>(orders, totalCount, page, pageSize, totalPages));
    }

    /// <summary>
    /// Lấy chi tiết đơn hàng của ngườI dùng hiện tại (US19)
    /// </summary>
    [HttpGet("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CustomerOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyOrderDetail([FromRoute] int id)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Không xác định được ngườI dùng." });
        }

        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Plant)
            .Include(o => o.StatusHistories.OrderByDescending(h => h.ChangedAt))
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound(new { message = "Không tìm thấy đơn hàng" });
        }

        var detailDto = new CustomerOrderDetailDto
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
                PlantName = oi.Plant?.Name ?? "Không xác định",
                ScientificName = oi.Plant?.ScientificName,
                ImageUrl = oi.Plant != null ? ImageUrlSanitizer.NormalizeOrEmpty(oi.Plant.ImageUrl) : null,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                LineTotal = oi.LineTotal
            }).ToList(),
            StatusHistory = order.StatusHistories.Select(h => new OrderStatusHistoryDto(
                h.Id,
                h.OldStatus.ToString(),
                h.NewStatus.ToString(),
                h.ChangedBy,
                h.Reason,
                h.ChangedAt)).ToList()
        };

        return Ok(detailDto);
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

    /// <summary>
    /// Hủy đơn hàng (US20) - Chỉ cho phép hủy đơn ở trạng thái Pending
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CancelOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder([FromRoute] int id, [FromBody] CancelOrderRequest? request)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Không xác định được ngườI dùng." });
        }

        var order = await _dbContext.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Plant)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound(new { message = "Không tìm thấy đơn hàng" });
        }

        // Chỉ cho phép hủy đơn ở trạng thái Pending
        if (order.Status != OrderStatus.Pending)
        {
            return BadRequest(new { message = $"Không thể hủy đơn hàng ở trạng thái '{order.Status}'. Chỉ có thể hủy đơn hàng ở trạng thái 'Chờ xử lý'." });
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var oldStatus = order.Status;
            var reason = request?.Reason ?? "Khách hàng yêu cầu hủy";

            // 1. Khôi phục tồn kho
            foreach (var item in order.OrderItems)
            {
                if (item.Plant != null)
                {
                    item.Plant.StockQuantity += item.Quantity;
                }
            }

            // 2. Cập nhật trạng thái đơn hàng
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = userId;

            // 3. Ghi log lịch sử thay đổi trạng thái
            var statusHistory = new OrderStatusHistory
            {
                OrderId = order.Id,
                OldStatus = oldStatus,
                NewStatus = OrderStatus.Cancelled,
                ChangedBy = userId,
                Reason = reason,
                ChangedAt = DateTime.UtcNow
            };
            _dbContext.OrderStatusHistories.Add(statusHistory);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Order {OrderId} cancelled by user {UserId}. Reason: {Reason}", id, userId, reason);

            return Ok(new CancelOrderResponse
            {
                OrderId = order.Id,
                Status = order.Status.ToString(),
                CancelledAt = statusHistory.ChangedAt,
                Reason = reason,
                Message = "Đơn hàng đã được hủy thành công."
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error cancelling order {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Có lỗi xảy ra khi hủy đơn hàng. Vui lòng thử lại sau." });
        }
    }
}

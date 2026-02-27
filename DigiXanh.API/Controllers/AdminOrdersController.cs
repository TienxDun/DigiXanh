using DigiXanh.API.Constants;
using DigiXanh.API.Data;
using DigiXanh.API.DTOs.Common;
using DigiXanh.API.DTOs.Orders;
using DigiXanh.API.Helpers;
using DigiXanh.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DigiXanh.API.Controllers;

/// <summary>
/// Controller quản lý đơn hàng cho Admin
/// </summary>
[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = DefaultRoles.Admin)]
public class AdminOrdersController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AdminOrdersController> _logger;

    public AdminOrdersController(
        ApplicationDbContext dbContext,
        ILogger<AdminOrdersController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách đơn hàng với phân trang, lọc và tìm kiếm
    /// </summary>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PagedResult<AdminOrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? status = null,
        [FromQuery] string? search = null)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize; // Giới hạn tối đa 100 items/page

        var query = _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .AsQueryable();

        // Lọc theo trạng thái
        if (status.HasValue && Enum.IsDefined(typeof(OrderStatus), status.Value))
        {
            query = query.Where(o => (int)o.Status == status.Value);
        }

        // Tìm kiếm theo tên khách hàng, email, SĐT, hoặc ID đơn hàng
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLower();
            
            // Kiểm tra nếu search là số (tìm theo ID)
            if (int.TryParse(search.Trim(), out var orderId))
            {
                query = query.Where(o => o.Id == orderId);
            }
            else
            {
                query = query.Where(o =>
                    (o.User != null && (o.User.FullName.ToLower().Contains(keyword) ||
                                       o.User.Email!.ToLower().Contains(keyword))) ||
                    o.Phone.Contains(keyword) ||
                    o.RecipientName.ToLower().Contains(keyword));
            }
        }

        var totalCount = await query.CountAsync();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new AdminOrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                CustomerName = o.User != null ? o.User.FullName : o.RecipientName,
                CustomerEmail = o.User != null ? o.User.Email! : "N/A",
                TotalAmount = o.TotalAmount,
                FinalAmount = o.FinalAmount,
                Status = o.Status.ToString(),
                PaymentMethod = o.PaymentMethod.ToString(),
                Phone = o.Phone
            })
            .ToListAsync();

        return Ok(new PagedResult<AdminOrderDto>(items, totalCount, page, pageSize, totalPages));
    }

    /// <summary>
    /// Lấy chi tiết đơn hàng bao gồm items và lịch sử trạng thái
    /// </summary>
    [HttpGet("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AdminOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderDetail([FromRoute] int id)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Plant)
            .Include(o => o.StatusHistories.OrderByDescending(h => h.ChangedAt))
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound(new { message = "Không tìm thấy đơn hàng" });
        }

        var detailDto = new AdminOrderDetailDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            CustomerName = order.User != null ? order.User.FullName : order.RecipientName,
            CustomerEmail = order.User != null ? order.User.Email! : "N/A",
            TotalAmount = order.TotalAmount,
            FinalAmount = order.FinalAmount,
            Status = order.Status.ToString(),
            PaymentMethod = order.PaymentMethod.ToString(),
            Phone = order.Phone,
            ShippingAddress = order.ShippingAddress,
            TransactionId = order.TransactionId,
            UpdatedAt = order.UpdatedAt,
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

    /// <summary>
    /// Cập nhật trạng thái đơn hàng
    /// </summary>
    [HttpPut("{id:int}/status")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateOrderStatus(
        [FromRoute] int id,
        [FromBody] UpdateOrderStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var order = await _dbContext.Orders
            .Include(o => o.StatusHistories)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound(new { message = "Không tìm thấy đơn hàng" });
        }

        var newStatus = (OrderStatus)request.NewStatus;
        var currentStatus = order.Status;

        // Kiểm tra nếu trạng thái không thay đổi
        if (currentStatus == newStatus)
        {
            return BadRequest(new { message = "Trạng thái mới phải khác trạng thái hiện tại" });
        }

        // Validate status transition
        if (!IsValidStatusTransition(currentStatus, newStatus))
        {
            return Conflict(new
            {
                message = $"Không thể chuyển từ trạng thái '{currentStatus}' sang '{newStatus}'",
                currentStatus = currentStatus.ToString(),
                allowedTransitions = GetAllowedTransitions(currentStatus)
            });
        }

        // Lấy thông tin admin hiện tại
        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Cập nhật trạng thái order
            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = adminEmail;

            // Thêm vào lịch sử
            var history = new OrderStatusHistory
            {
                OrderId = order.Id,
                OldStatus = currentStatus,
                NewStatus = newStatus,
                ChangedBy = adminEmail,
                Reason = request.Reason,
                ChangedAt = DateTime.UtcNow
            };

            _dbContext.OrderStatusHistories.Add(history);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Order {OrderId} status updated from {OldStatus} to {NewStatus} by {Admin}",
                order.Id, currentStatus, newStatus, adminEmail);

            return Ok(new
            {
                message = "Cập nhật trạng thái thành công",
                orderId = order.Id,
                oldStatus = currentStatus.ToString(),
                newStatus = newStatus.ToString(),
                updatedAt = order.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating order {OrderId} status", id);
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách trạng thái đơn hàng (để FE hiển thị dropdown filter)
    /// </summary>
    [HttpGet("statuses")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetOrderStatuses()
    {
        var statuses = Enum.GetValues<OrderStatus>()
            .Select(s => new
            {
                Value = (int)s,
                Name = s.ToString(),
                DisplayName = GetVietnameseStatusName(s)
            })
            .ToList();

        return Ok(statuses);
    }

    #region Helper Methods

    /// <summary>
    /// Kiểm tra transition trạng thái có hợp lệ không
    /// </summary>
    private static bool IsValidStatusTransition(OrderStatus current, OrderStatus next)
    {
        return current switch
        {
            OrderStatus.Pending => next is OrderStatus.Paid or OrderStatus.Cancelled,
            OrderStatus.Paid => next is OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Shipped => next is OrderStatus.Delivered or OrderStatus.Cancelled,
            OrderStatus.Delivered => false, // Final state, không thể chuyển
            OrderStatus.Cancelled => false, // Final state, không thể chuyển
            _ => false
        };
    }

    /// <summary>
    /// Lấy danh sách trạng thái có thể chuyển sang từ trạng thái hiện tại
    /// </summary>
    private static List<string> GetAllowedTransitions(OrderStatus current)
    {
        return current switch
        {
            OrderStatus.Pending => new List<string> { OrderStatus.Paid.ToString(), OrderStatus.Cancelled.ToString() },
            OrderStatus.Paid => new List<string> { OrderStatus.Shipped.ToString(), OrderStatus.Cancelled.ToString() },
            OrderStatus.Shipped => new List<string> { OrderStatus.Delivered.ToString(), OrderStatus.Cancelled.ToString() },
            OrderStatus.Delivered => new List<string>(),
            OrderStatus.Cancelled => new List<string>(),
            _ => new List<string>()
        };
    }

    /// <summary>
    /// Chuyển đổi tên trạng thái sang tiếng Việt
    /// </summary>
    private static string GetVietnameseStatusName(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Chờ xử lý",
            OrderStatus.Paid => "Đã thanh toán",
            OrderStatus.Shipped => "Đang giao",
            OrderStatus.Delivered => "Đã giao",
            OrderStatus.Cancelled => "Đã hủy",
            _ => status.ToString()
        };
    }

    #endregion
}

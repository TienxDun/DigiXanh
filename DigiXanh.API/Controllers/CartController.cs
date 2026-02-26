using System.Security.Claims;
using DigiXanh.API.Data;
using DigiXanh.API.DTOs.Cart;
using DigiXanh.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private const int MaxQuantityPerItem = 99;
    private readonly ApplicationDbContext _dbContext;

    public CartController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CartSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCart()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Không xác định được người dùng." });
        }

        var userExists = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Id == userId);

        if (!userExists)
        {
            return Unauthorized(new { message = "Phiên đăng nhập không còn hợp lệ, vui lòng đăng nhập lại." });
        }

        var items = await _dbContext.CartItems
            .AsNoTracking()
            .Include(item => item.Plant)
            .Where(item => item.UserId == userId && item.Plant != null && !item.Plant.IsDeleted)
            .OrderByDescending(item => item.UpdatedAt)
            .Select(item => new CartItemDto(
                item.Id,
                item.PlantId,
                item.Plant!.Name,
                item.Plant.ScientificName,
                item.Plant.Price,
                item.Plant.ImageUrl,
                item.Quantity,
                item.Quantity * item.Plant.Price))
            .ToListAsync();

        var totalQuantity = items.Sum(item => item.Quantity);
        var totalAmount = items.Sum(item => item.LineTotal);

        return Ok(new CartSummaryDto(items, totalQuantity, totalAmount));
    }

    [HttpPost("items")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CartSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
    {
        if (request.PlantId <= 0)
        {
            return BadRequest(new { message = "PlantId không hợp lệ." });
        }

        if (request.Quantity < 1 || request.Quantity > MaxQuantityPerItem)
        {
            return BadRequest(new { message = $"Số lượng phải từ 1 đến {MaxQuantityPerItem}." });
        }

        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Không xác định được người dùng." });
        }

        var userExists = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Id == userId);

        if (!userExists)
        {
            return Unauthorized(new { message = "Phiên đăng nhập không còn hợp lệ, vui lòng đăng nhập lại." });
        }

        var plant = await _dbContext.Plants
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.PlantId && !item.IsDeleted);

        if (plant is null)
        {
            return NotFound(new { message = "Không tìm thấy cây." });
        }

        var existingItem = await _dbContext.CartItems
            .FirstOrDefaultAsync(item => item.UserId == userId && item.PlantId == request.PlantId);

        if (existingItem is null)
        {
            _dbContext.CartItems.Add(new CartItem
            {
                UserId = userId,
                PlantId = request.PlantId,
                Quantity = request.Quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existingItem.Quantity = Math.Min(MaxQuantityPerItem, existingItem.Quantity + request.Quantity);
            existingItem.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        return await GetMyCart();
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue(ClaimTypes.Name)
               ?? User.FindFirstValue("sub");
    }
}
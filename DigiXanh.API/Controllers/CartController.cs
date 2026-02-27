using System.Security.Claims;
using DigiXanh.API.Data;
using DigiXanh.API.DTOs.Cart;
using DigiXanh.API.Helpers;
using DigiXanh.API.Models;
using DigiXanh.API.Patterns.Decorator;
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
    private readonly IPriceCalculator _priceCalculator;

    public CartController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _priceCalculator = PriceCalculatorFactory.CreateCalculatorWithDiscounts();
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
                ImageUrlSanitizer.NormalizeOrEmpty(item.Plant.ImageUrl),
                item.Quantity,
                item.Quantity * item.Plant.Price))
            .ToListAsync();

        var totalQuantity = items.Sum(item => item.Quantity);
        var cartItems = items.Select(dto => new CartItem
        {
            Id = dto.Id,
            PlantId = dto.PlantId,
            Quantity = dto.Quantity,
            Plant = new Plant
            {
                Id = dto.PlantId,
                Name = dto.PlantName,
                ScientificName = dto.ScientificName,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl
            }
        }).ToList();

        var (baseAmount, discountAmount, finalAmount) = _priceCalculator.CalculatePriceWithDetails(cartItems);
        var discountPercent = totalQuantity >= 3 ? 7 : totalQuantity >= 2 ? 5 : 0;

        return Ok(new CartSummaryDto(items, totalQuantity, baseAmount, discountAmount, discountPercent, finalAmount));
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

    [HttpPut("items/{cartItemId:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CartSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, [FromBody] UpdateCartItemQuantityRequest request)
    {
        if (cartItemId <= 0)
        {
            return BadRequest(new { message = "Cart item không hợp lệ." });
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

        var item = await _dbContext.CartItems
            .Include(cartItem => cartItem.Plant)
            .FirstOrDefaultAsync(cartItem => cartItem.Id == cartItemId && cartItem.UserId == userId);

        if (item is null || item.Plant is null || item.Plant.IsDeleted)
        {
            return NotFound(new { message = "Không tìm thấy sản phẩm trong giỏ hàng." });
        }

        item.Quantity = request.Quantity;
        item.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return await GetMyCart();
    }

    [HttpDelete("items/{cartItemId:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CartSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        if (cartItemId <= 0)
        {
            return BadRequest(new { message = "Cart item không hợp lệ." });
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

        var item = await _dbContext.CartItems
            .FirstOrDefaultAsync(cartItem => cartItem.Id == cartItemId && cartItem.UserId == userId);

        if (item is null)
        {
            return NotFound(new { message = "Không tìm thấy sản phẩm trong giỏ hàng." });
        }

        _dbContext.CartItems.Remove(item);
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
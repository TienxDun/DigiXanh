using System.Security.Claims;
using DigiXanh.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiXanh.API.Controllers;

/// <summary>
/// Base controller cung cấp các phương thức chung cho tất cả controllers
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    protected string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue(ClaimTypes.Name)
               ?? User.FindFirstValue("sub");
    }

    protected async Task<bool> ValidateUserExistsAsync(ApplicationDbContext dbContext, string userId)
    {
        return await dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Id == userId);
    }

    protected IActionResult UserNotFoundResult()
    {
        return Unauthorized(new { message = "Không xác định được ngườI dùng." });
    }

    protected IActionResult UserSessionInvalidResult()
    {
        return Unauthorized(new { message = "Phiên đăng nhập không còn hợp lệ, vui lòng đăng nhập lại." });
    }

    protected string GetIpAddress()
    {
        var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ip))
        {
            ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        }
        return ip.Split(',')[0].Trim();
    }
}

using System.Text.RegularExpressions;
using DigiXanh.API.DTOs.Common;
using DigiXanh.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiXanh.API.Controllers;

/// <summary>
/// Controller quản lý thông tin ngườI dùng (US21)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserController> _logger;

    public UserController(
        UserManager<ApplicationUser> userManager,
        ILogger<UserController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Lấy thông tin cá nhân của ngườI dùng hiện tại
    /// </summary>
    [HttpGet("profile")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Không xác định được ngườI dùng." });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy ngườI dùng." });
        }

        var profile = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            CreatedAt = user.CreatedAt
        };

        return Ok(profile);
    }

    /// <summary>
    /// Cập nhật thông tin cá nhân (US21)
    /// </summary>
    [HttpPut("profile")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Không xác định được ngườI dùng." });
        }

        // Validate request
        var validationErrors = ValidateUpdateProfileRequest(request);
        if (validationErrors.Any())
        {
            return BadRequest(new ValidationErrorResponse(validationErrors));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy ngườI dùng." });
        }

        // Cập nhật thông tin
        user.FullName = request.FullName?.Trim() ?? user.FullName;
        user.PhoneNumber = request.PhoneNumber?.Trim() ?? user.PhoneNumber;
        user.Address = request.Address?.Trim() ?? user.Address;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(
                e => e.Code.Contains("Phone") ? "phoneNumber" : "general",
                e => new[] { e.Description });
            return BadRequest(new ValidationErrorResponse(errors));
        }

        _logger.LogInformation("User {UserId} updated profile", userId);

        var updatedProfile = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            CreatedAt = user.CreatedAt
        };

        return Ok(updatedProfile);
    }

    /// <summary>
    /// Validate SĐT Việt Nam và các trường khác
    /// </summary>
    private static Dictionary<string, string[]> ValidateUpdateProfileRequest(UpdateProfileRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        // Validate FullName
        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            var fullName = request.FullName.Trim();
            if (fullName.Length < 2)
            {
                errors["fullName"] = new[] { "Họ tên phải có ít nhất 2 ký tự." };
            }
            else if (fullName.Length > 200)
            {
                errors["fullName"] = new[] { "Họ tên không được vượt quá 200 ký tự." };
            }
        }

        // Validate PhoneNumber (SĐT Việt Nam)
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var phone = request.PhoneNumber.Trim();
            
            // Regex cho SĐT Việt Nam:
            // - Bắt đầu bằng 0 hoặc +84
            // - Theo sau là 9 chữ số (nếu bắt đầu bằng 0) hoặc 9 chữ số (nếu bắt đầu bằng +84)
            // - Các đầu số hợp lệ: 03, 05, 07, 08, 09
            var vietnamPhoneRegex = new Regex(@"^(0|84|\+84)(3|5|7|8|9)\d{8}$");
            
            // Chuẩn hóa SĐT trước khi check
            var normalizedPhone = phone.StartsWith("+") ? phone.Substring(1) : phone;
            
            if (!vietnamPhoneRegex.IsMatch(normalizedPhone))
            {
                errors["phoneNumber"] = new[] { "Số điện thoại không hợp lệ. Vui lòng nhập SĐT Việt Nam (VD: 0901234567 hoặc +84901234567)." };
            }
        }

        // Validate Address
        if (!string.IsNullOrWhiteSpace(request.Address))
        {
            var address = request.Address.Trim();
            if (address.Length > 500)
            {
                errors["address"] = new[] { "Địa chỉ không được vượt quá 500 ký tự." };
            }
        }

        return errors;
    }
}

/// <summary>
/// DTO thông tin cá nhân ngườI dùng
/// </summary>
public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request cập nhật thông tin cá nhân
/// </summary>
public class UpdateProfileRequest
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}

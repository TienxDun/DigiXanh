using DigiXanh.API.Constants;
using DigiXanh.API.DTOs.Common;
using DigiXanh.API.DTOs.Users;
using DigiXanh.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiXanh.API.Controllers;

/// <summary>
/// Controller quản lý ngườI dùng cho Admin (US27)
/// </summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = DefaultRoles.Admin)]
public class AdminUsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<AdminUsersController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách ngườI dùng với phân trang và tìm kiếm
    /// </summary>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PagedResult<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        // Lấy tất cả users (không bao gồm admin hiện tại)
        var currentAdminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        var query = _userManager.Users
            .AsNoTracking()
            .Where(u => u.Id != currentAdminId) // Không hiển thị chính mình
            .AsQueryable();

        // Tìm kiếm theo tên, email, SĐT
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(keyword) ||
                (u.Email != null && u.Email.ToLower().Contains(keyword)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(keyword)));
        }

        var totalCount = await query.CountAsync();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Lấy roles cho từng user
        var userDtos = new List<AdminUserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            // Filter theo role nếu có
            if (!string.IsNullOrWhiteSpace(role) && !roles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            var isLocked = await _userManager.IsLockedOutAsync(user);

            userDtos.Add(new AdminUserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsLocked = isLocked,
                Roles = roles.ToList()
            });
        }

        // Recalculate total if filtered by role
        if (!string.IsNullOrWhiteSpace(role))
        {
            totalCount = userDtos.Count;
            totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        return Ok(new PagedResult<AdminUserDto>(userDtos, totalCount, page, pageSize, totalPages));
    }

    /// <summary>
    /// Lấy chi tiết ngườI dùng
    /// </summary>
    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AdminUserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserDetail([FromRoute] string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy ngườI dùng" });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var isLocked = await _userManager.IsLockedOutAsync(user);

        // Lấy thông tin đơn hàng từ DbContext
        // Note: Cần inject DbContext nếu muốn lấy chi tiết đơn hàng
        // Hiện tại trả về thông tin cơ bản

        var detailDto = new AdminUserDetailDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsLocked = isLocked,
            Roles = roles.ToList(),
            TotalOrders = 0, // Có thể bổ sung sau khi có DbContext
            TotalSpent = 0,
            RecentOrders = new List<UserOrderSummaryDto>()
        };

        return Ok(detailDto);
    }

    /// <summary>
    /// Khóa hoặc mở khóa tài khoản ngườI dùng
    /// </summary>
    [HttpPut("{id}/lock")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserLockout(
        [FromRoute] string id,
        [FromBody] UpdateUserLockoutRequest request)
    {
        // Không cho phép tự khóa chính mình
        var currentAdminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (id == currentAdminId)
        {
            return BadRequest(new { message = "Không thể khóa tài khoản của chính mình" });
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy ngườI dùng" });
        }

        // Kiểm tra nếu user là admin khác thì không cho khóa
        var userRoles = await _userManager.GetRolesAsync(user);
        if (userRoles.Contains(DefaultRoles.Admin))
        {
            return BadRequest(new { message = "Không thể khóa tài khoản Admin khác" });
        }

        IdentityResult result;
        if (request.IsLocked)
        {
            // Khóa tài khoản vĩnh viễn (đến năm 9999)
            result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            _logger.LogInformation("User {UserId} has been locked by {AdminId}. Reason: {Reason}", 
                id, currentAdminId, request.Reason ?? "N/A");
        }
        else
        {
            // Mở khóa
            result = await _userManager.SetLockoutEndDateAsync(user, null);
            result = await _userManager.ResetAccessFailedCountAsync(user);
            _logger.LogInformation("User {UserId} has been unlocked by {AdminId}", 
                id, currentAdminId);
        }

        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Cập nhật trạng thái thất bại", errors = result.Errors });
        }

        return Ok(new
        {
            message = request.IsLocked ? "Đã khóa tài khoản thành công" : "Đã mở khóa tài khoản thành công",
            userId = id,
            isLocked = request.IsLocked
        });
    }

    /// <summary>
    /// Cập nhật role của ngườI dùng
    /// </summary>
    [HttpPut("{id}/role")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRole(
        [FromRoute] string id,
        [FromBody] UpdateUserRoleRequest request)
    {
        // Validate role
        if (string.IsNullOrWhiteSpace(request.Role) || 
            (!request.Role.Equals(DefaultRoles.Admin, StringComparison.OrdinalIgnoreCase) && 
             !request.Role.Equals(DefaultRoles.User, StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(new { message = "Role không hợp lệ. Chỉ chấp nhận 'Admin' hoặc 'User'" });
        }

        var normalizedRole = request.Role.Equals(DefaultRoles.Admin, StringComparison.OrdinalIgnoreCase) 
            ? DefaultRoles.Admin 
            : DefaultRoles.User;

        // Không cho phép tự đổi role chính mình
        var currentAdminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (id == currentAdminId)
        {
            return BadRequest(new { message = "Không thể thay đổi role của chính mình" });
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy ngườI dùng" });
        }

        // Lấy roles hiện tại
        var currentRoles = await _userManager.GetRolesAsync(user);

        // Nếu đã có role này rồi thì báo lỗi
        if (currentRoles.Contains(normalizedRole))
        {
            return BadRequest(new { message = $"NgườI dùng đã có role '{normalizedRole}'" });
        }

        // Xóa tất cả roles cũ và thêm role mới
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            return BadRequest(new { message = "Xóa role cũ thất bại", errors = removeResult.Errors });
        }

        var addResult = await _userManager.AddToRoleAsync(user, normalizedRole);
        if (!addResult.Succeeded)
        {
            // Rollback: thử thêm lại roles cũ
            await _userManager.AddToRolesAsync(user, currentRoles);
            return BadRequest(new { message = "Thêm role mới thất bại", errors = addResult.Errors });
        }

        _logger.LogInformation("User {UserId} role changed from {OldRoles} to {NewRole} by {AdminId}",
            id, string.Join(",", currentRoles), normalizedRole, currentAdminId);

        return Ok(new
        {
            message = "Cập nhật role thành công",
            userId = id,
            oldRoles = currentRoles,
            newRole = normalizedRole
        });
    }

    /// <summary>
    /// Lấy danh sách các roles có sẵn
    /// </summary>
    [HttpGet("roles")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleManager.Roles
            .Select(r => new RoleDto
            {
                Name = r.Name ?? string.Empty,
                DisplayName = r.Name == DefaultRoles.Admin ? "Quản trị viên" : "NgườI dùng"
            })
            .ToListAsync();

        // Đảm bảo luôn có 2 roles mặc định
        if (roles.All(r => r.Name != DefaultRoles.Admin))
        {
            roles.Add(new RoleDto { Name = DefaultRoles.Admin, DisplayName = "Quản trị viên" });
        }
        if (roles.All(r => r.Name != DefaultRoles.User))
        {
            roles.Add(new RoleDto { Name = DefaultRoles.User, DisplayName = "NgườI dùng" });
        }

        return Ok(roles);
    }
}

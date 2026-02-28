namespace DigiXanh.API.DTOs.Users;

/// <summary>
/// DTO cho danh sách user trong admin
/// </summary>
public class AdminUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsLocked { get; set; }
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// DTO chi tiết user cho admin
/// </summary>
public class AdminUserDetailDto : AdminUserDto
{
    public List<UserOrderSummaryDto> RecentOrders { get; set; } = new();
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
}

/// <summary>
/// DTO tóm tắt đơn hàng của user
/// </summary>
public class UserOrderSummaryDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Request cập nhật trạng thái khóa tài khoản
/// </summary>
public class UpdateUserLockoutRequest
{
    /// <summary>
    /// true = khóa, false = mở khóa
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Lý do khóa/mở khóa (optional)
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Request cập nhật role của user
/// </summary>
public class UpdateUserRoleRequest
{
    /// <summary>
    /// Role mới: "Admin" hoặc "User"
    /// </summary>
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// DTO thông tin role
/// </summary>
public class RoleDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

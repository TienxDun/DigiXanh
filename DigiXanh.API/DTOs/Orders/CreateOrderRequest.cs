using System.ComponentModel.DataAnnotations;

namespace DigiXanh.API.DTOs.Orders;

public class CreateOrderRequest
{
    [Required(ErrorMessage = "Vui lòng nhập tên ngườI nhận")]
    [MinLength(2, ErrorMessage = "Tên quá ngắn")]
    [MaxLength(200, ErrorMessage = "Tên quá dài")]
    public string RecipientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
    [MinLength(10, ErrorMessage = "Địa chỉ quá ngắn")]
    [MaxLength(500, ErrorMessage = "Địa chỉ quá dài")]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
    [Range(0, 1, ErrorMessage = "Phương thức thanh toán không hợp lệ")]
    public PaymentMethodDto PaymentMethod { get; set; }

    public string? ReturnUrl { get; set; }
}

public enum PaymentMethodDto
{
    Cash = 0,
    VNPay = 1
}

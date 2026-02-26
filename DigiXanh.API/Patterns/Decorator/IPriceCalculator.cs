using DigiXanh.API.Models;

namespace DigiXanh.API.Patterns.Decorator;

public interface IPriceCalculator
{
    decimal CalculatePrice(IEnumerable<CartItem> items);
    (decimal baseAmount, decimal discountAmount, decimal finalAmount) CalculatePriceWithDetails(IEnumerable<CartItem> items);
}

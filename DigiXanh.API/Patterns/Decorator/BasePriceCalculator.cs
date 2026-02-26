using DigiXanh.API.Models;

namespace DigiXanh.API.Patterns.Decorator;

public class BasePriceCalculator : IPriceCalculator
{
    public decimal CalculatePrice(IEnumerable<CartItem> items)
    {
        return items.Sum(i => i.Quantity * (i.Plant?.Price ?? 0));
    }

    public (decimal baseAmount, decimal discountAmount, decimal finalAmount) CalculatePriceWithDetails(IEnumerable<CartItem> items)
    {
        var baseAmount = CalculatePrice(items);
        return (baseAmount, 0, baseAmount);
    }
}

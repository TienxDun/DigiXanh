using DigiXanh.API.Models;

namespace DigiXanh.API.Patterns.Decorator;

public class QuantityDiscountDecorator : IPriceCalculator
{
    private readonly IPriceCalculator _inner;
    private readonly int _threshold;
    private readonly decimal _discountPercent;

    public QuantityDiscountDecorator(IPriceCalculator inner, int threshold, decimal discountPercent)
    {
        _inner = inner;
        _threshold = threshold;
        _discountPercent = discountPercent;
    }

    public decimal CalculatePrice(IEnumerable<CartItem> items)
    {
        var baseTotal = _inner.CalculatePrice(items);
        var totalQuantity = items.Sum(i => i.Quantity);

        if (totalQuantity >= _threshold)
        {
            return baseTotal * (1 - _discountPercent / 100);
        }
        return baseTotal;
    }

    public (decimal baseAmount, decimal discountAmount, decimal finalAmount) CalculatePriceWithDetails(IEnumerable<CartItem> items)
    {
        var (innerBase, innerDiscount, innerFinal) = _inner.CalculatePriceWithDetails(items);
        var totalQuantity = items.Sum(i => i.Quantity);

        if (totalQuantity >= _threshold)
        {
            // Nếu decorator bên trong đã áp dụng giảm giá, so sánh và chọn mức cao nhất
            var newDiscountAmount = innerBase * (_discountPercent / 100);
            var currentTotalDiscount = innerDiscount;

            // Chọn mức giảm cao nhất
            if (newDiscountAmount > currentTotalDiscount)
            {
                var finalAmount = innerBase - newDiscountAmount;
                return (innerBase, newDiscountAmount, finalAmount);
            }
        }

        return (innerBase, innerDiscount, innerFinal);
    }
}

public static class PriceCalculatorFactory
{
    public static IPriceCalculator CreateCalculatorWithDiscounts()
    {
        IPriceCalculator calculator = new BasePriceCalculator();
        // Ưu tiên mức giảm cao hơn (>=3 giảm 7%) trước, sau đó mới đến mức thấp hơn (>=2 giảm 5%)
        calculator = new QuantityDiscountDecorator(calculator, 3, 7); // Giảm 7% nếu mua >=3
        calculator = new QuantityDiscountDecorator(calculator, 2, 5); // Giảm 5% nếu mua >=2
        return calculator;
    }
}

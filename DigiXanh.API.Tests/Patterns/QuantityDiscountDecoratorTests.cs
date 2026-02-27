using DigiXanh.API.Models;
using DigiXanh.API.Patterns.Decorator;

namespace DigiXanh.API.Tests.Patterns;

public class QuantityDiscountDecoratorTests
{
    #region BasePriceCalculator Tests

    [Fact]
    public void CalculatePrice_ReturnsCorrectSum_WhenGivenMultipleItems()
    {
        // Arrange
        var calculator = new BasePriceCalculator();
        var items = new List<CartItem>
        {
            new() { Id = 1, PlantId = 1, Quantity = 2, Plant = new Plant { Id = 1, Price = 100 } },
            new() { Id = 2, PlantId = 2, Quantity = 3, Plant = new Plant { Id = 2, Price = 50 } }
        };

        // Act
        var result = calculator.CalculatePrice(items);

        // Assert
        // (2 * 100) + (3 * 50) = 200 + 150 = 350
        Assert.Equal(350m, result);
    }

    [Fact]
    public void CalculatePrice_ReturnsZero_WhenItemsListIsEmpty()
    {
        // Arrange
        var calculator = new BasePriceCalculator();
        var items = new List<CartItem>();

        // Act
        var result = calculator.CalculatePrice(items);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculatePrice_ReturnsZero_WhenPlantIsNull()
    {
        // Arrange
        var calculator = new BasePriceCalculator();
        var items = new List<CartItem>
        {
            new() { Id = 1, PlantId = 1, Quantity = 2, Plant = null }
        };

        // Act
        var result = calculator.CalculatePrice(items);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculatePriceWithDetails_ReturnsBaseZeroDiscountAndFinal_WhenNoDiscountApplied()
    {
        // Arrange
        var calculator = new BasePriceCalculator();
        var items = new List<CartItem>
        {
            new() { Id = 1, PlantId = 1, Quantity = 2, Plant = new Plant { Id = 1, Price = 100 } }
        };

        // Act
        var (baseAmount, discountAmount, finalAmount) = calculator.CalculatePriceWithDetails(items);

        // Assert
        Assert.Equal(200m, baseAmount);
        Assert.Equal(0m, discountAmount);
        Assert.Equal(200m, finalAmount);
    }

    #endregion

    #region QuantityDiscountDecorator Tests

    [Fact]
    public void CalculatePrice_ReturnsBasePrice_WhenTotalQuantityIsLessThanThreshold()
    {
        // Arrange
        var baseCalculator = new BasePriceCalculator();
        var decorator = new QuantityDiscountDecorator(baseCalculator, threshold: 2, discountPercent: 5);
        var items = new List<CartItem>
        {
            new() { Id = 1, PlantId = 1, Quantity = 1, Plant = new Plant { Id = 1, Price = 100 } }
        };

        // Act
        var result = decorator.CalculatePrice(items);

        // Assert
        // Quantity = 1 (< 2), so no discount should be applied
        Assert.Equal(100m, result);
    }

    [Fact]
    public void CalculatePrice_AppliesFivePercentDiscount_WhenTotalQuantityEqualsTwo()
    {
        // Arrange
        var baseCalculator = new BasePriceCalculator();
        var decorator = new QuantityDiscountDecorator(baseCalculator, threshold: 2, discountPercent: 5);
        var items = new List<CartItem>
        {
            new() { Id = 1, PlantId = 1, Quantity = 2, Plant = new Plant { Id = 1, Price = 100 } }
        };

        // Act
        var result = decorator.CalculatePrice(items);

        // Assert
        // Base: 2 * 100 = 200, Discount: 5%, Final: 200 * 0.95 = 190
        Assert.Equal(190m, result);
    }

    [Fact]
    public void CalculatePrice_AppliesSevenPercentDiscount_WhenTotalQuantityEqualsThree()
    {
        // Arrange
        var baseCalculator = new BasePriceCalculator();
        var decorator = new QuantityDiscountDecorator(baseCalculator, threshold: 3, discountPercent: 7);
        var items = new List<CartItem>
        {
            new() { Id = 1, PlantId = 1, Quantity = 3, Plant = new Plant { Id = 1, Price = 100 } }
        };

        // Act
        var result = decorator.CalculatePrice(items);

        // Assert
        // Base: 3 * 100 = 300, Discount: 7%, Final: 300 * 0.93 = 279
        Assert.Equal(279m, result);
    }

    [Fact]
    public void CalculatePrice_AppliesSevenPercentDiscount_WhenTotalQuantityGreaterThanThree()
    {
        // Arrange
        var baseCalculator = new BasePriceCalculator();
        var decorator = new QuantityDiscountDecorator(baseCalculator, threshold: 3, discountPercent: 7);
        var items = new List<CartItem>
        {
            new() { Id = 1, PlantId = 1, Quantity = 5, Plant = new Plant { Id = 1, Price = 100 } }
        };

        // Act
        var result = decorator.CalculatePrice(items);

        // Assert
        // Base: 5 * 100 = 500, Discount: 7%, Final: 500 * 0.93 = 465
        Assert.Equal(465m, result);
    }

    [Fact]
    public void CalculatePrice_CalculatesAcrossMultipleItems()
    {
        // Arrange
        var baseCalculator = new BasePriceCalculator();
        var decorator = new QuantityDiscountDecorator(baseCalculator, threshold: 2, discountPercent: 5);
        var items = new List<CartItem>
        {
            new() { Id = 1, PlantId = 1, Quantity = 1, Plant = new Plant { Id = 1, Price = 100 } },
            new() { Id = 2, PlantId = 2, Quantity = 1, Plant = new Plant { Id = 2, Price = 50 } }
        };

        // Act
        var result = decorator.CalculatePrice(items);

        // Assert
        // Total Quantity = 2, Base: (1 * 100) + (1 * 50) = 150, Discount: 5%, Final: 150 * 0.95 = 142.5
        Assert.Equal(142.5m, result);
    }

    [Fact]
    public void CalculatePriceWithDetails_ReturnsCorrectValues_WhenNoDiscountApplied()
    {
        // Arrange
        var baseCalculator = new BasePriceCalculator();
        var decorator = new QuantityDiscountDecorator(baseCalculator, threshold: 2, discountPercent: 5);
        var items = new List<CartItem>
        {
            new() { Id = 1, PlantId = 1, Quantity = 1, Plant = new Plant { Id = 1, Price = 100 } }
        };

        // Act
        var (baseAmount, discountAmount, finalAmount) = decorator.CalculatePriceWithDetails(items);

        // Assert
        Assert.Equal(100m, baseAmount);
        Assert.Equal(0m, discountAmount);
        Assert.Equal(100m, finalAmount);
    }

    [Fact]
    public void CalculatePriceWithDetails_ReturnsCorrectValues_WhenDiscountApplied()
    {
        // Arrange
        var baseCalculator = new BasePriceCalculator();
        var decorator = new QuantityDiscountDecorator(baseCalculator, threshold: 2, discountPercent: 5);
        var items = new List<CartItem>
        {
            new() { Id = 1, PlantId = 1, Quantity = 2, Plant = new Plant { Id = 1, Price = 100 } }
        };

        // Act
        var (baseAmount, discountAmount, finalAmount) = decorator.CalculatePriceWithDetails(items);

        // Assert
        // Base: 2 * 100 = 200, Discount: 200 * 0.05 = 10, Final: 190
        Assert.Equal(200m, baseAmount);
        Assert.Equal(10m, discountAmount);
        Assert.Equal(190m, finalAmount);
    }

    [Fact]
    public void CalculatePriceWithDetails_ChoosesHigherDiscount_WhenMultipleDecoratorsChained()
    {
        // Arrange - Chain decorators: inner with 5% at threshold 2, outer with 7% at threshold 3
        var baseCalculator = new BasePriceCalculator();
        var innerDecorator = new QuantityDiscountDecorator(baseCalculator, threshold: 2, discountPercent: 5);
        var outerDecorator = new QuantityDiscountDecorator(innerDecorator, threshold: 3, discountPercent: 7);
        
        var items = new List<CartItem>
        {
            new() { Id = 1, PlantId = 1, Quantity = 3, Plant = new Plant { Id = 1, Price = 100 } }
        };

        // Act
        var (baseAmount, discountAmount, finalAmount) = outerDecorator.CalculatePriceWithDetails(items);

        // Assert
        // Base: 3 * 100 = 300
        // Inner discount (5%): 300 * 0.05 = 15
        // Outer discount (7%): 300 * 0.07 = 21 (higher, so chosen)
        // Final: 300 - 21 = 279
        Assert.Equal(300m, baseAmount);
        Assert.Equal(21m, discountAmount);
        Assert.Equal(279m, finalAmount);
    }

    [Fact]
    public void CalculatePriceWithDetails_KeepsLowerDiscount_WhenHigherThresholdNotMet()
    {
        // Arrange - Chain decorators: inner with 5% at threshold 2, outer with 7% at threshold 3
        var baseCalculator = new BasePriceCalculator();
        var innerDecorator = new QuantityDiscountDecorator(baseCalculator, threshold: 2, discountPercent: 5);
        var outerDecorator = new QuantityDiscountDecorator(innerDecorator, threshold: 3, discountPercent: 7);
        
        var items = new List<CartItem>
        {
            new() { Id = 1, PlantId = 1, Quantity = 2, Plant = new Plant { Id = 1, Price = 100 } }
        };

        // Act
        var (baseAmount, discountAmount, finalAmount) = outerDecorator.CalculatePriceWithDetails(items);

        // Assert
        // Base: 2 * 100 = 200
        // Quantity = 2, only inner decorator (5%) applies, outer (7%) doesn't apply
        Assert.Equal(200m, baseAmount);
        Assert.Equal(10m, discountAmount);
        Assert.Equal(190m, finalAmount);
    }

    #endregion

    #region PriceCalculatorFactory Tests

    [Fact]
    public void CreateCalculatorWithDiscounts_ReturnsCalculatorWithProperDecoratorChain()
    {
        // Act
        var calculator = PriceCalculatorFactory.CreateCalculatorWithDiscounts();

        // Assert
        Assert.NotNull(calculator);
        Assert.IsAssignableFrom<IPriceCalculator>(calculator);
    }

    [Fact]
    public void CreateCalculatorWithDiscounts_AppliesFivePercentDiscount_WhenQuantityEqualsTwo()
    {
        // Arrange
        var calculator = PriceCalculatorFactory.CreateCalculatorWithDiscounts();
        var items = new List<CartItem>
        {
            new() { Id = 1, PlantId = 1, Quantity = 2, Plant = new Plant { Id = 1, Price = 100 } }
        };

        // Act
        var (baseAmount, discountAmount, finalAmount) = calculator.CalculatePriceWithDetails(items);

        // Assert
        // Base: 2 * 100 = 200, Discount: 5% = 10, Final: 190
        Assert.Equal(200m, baseAmount);
        Assert.Equal(10m, discountAmount);
        Assert.Equal(190m, finalAmount);
    }

    [Fact]
    public void CreateCalculatorWithDiscounts_AppliesSevenPercentDiscount_WhenQuantityEqualsThree()
    {
        // Arrange
        var calculator = PriceCalculatorFactory.CreateCalculatorWithDiscounts();
        var items = new List<CartItem>
        {
            new() { Id = 1, PlantId = 1, Quantity = 3, Plant = new Plant { Id = 1, Price = 100 } }
        };

        // Act
        var (baseAmount, discountAmount, finalAmount) = calculator.CalculatePriceWithDetails(items);

        // Assert
        // Base: 3 * 100 = 300, Discount: 7% = 21, Final: 279
        Assert.Equal(300m, baseAmount);
        Assert.Equal(21m, discountAmount);
        Assert.Equal(279m, finalAmount);
    }

    [Fact]
    public void CreateCalculatorWithDiscounts_AppliesSevenPercentDiscount_WhenQuantityGreaterThanThree()
    {
        // Arrange
        var calculator = PriceCalculatorFactory.CreateCalculatorWithDiscounts();
        var items = new List<CartItem>
        {
            new() { Id = 1, PlantId = 1, Quantity = 5, Plant = new Plant { Id = 1, Price = 100 } }
        };

        // Act
        var (baseAmount, discountAmount, finalAmount) = calculator.CalculatePriceWithDetails(items);

        // Assert
        // Base: 5 * 100 = 500, Discount: 7% = 35, Final: 465
        Assert.Equal(500m, baseAmount);
        Assert.Equal(35m, discountAmount);
        Assert.Equal(465m, finalAmount);
    }

    [Fact]
    public void CreateCalculatorWithDiscounts_AppliesNoDiscount_WhenQuantityIsOne()
    {
        // Arrange
        var calculator = PriceCalculatorFactory.CreateCalculatorWithDiscounts();
        var items = new List<CartItem>
        {
            new() { Id = 1, PlantId = 1, Quantity = 1, Plant = new Plant { Id = 1, Price = 100 } }
        };

        // Act
        var (baseAmount, discountAmount, finalAmount) = calculator.CalculatePriceWithDetails(items);

        // Assert
        // Base: 1 * 100 = 100, No discount
        Assert.Equal(100m, baseAmount);
        Assert.Equal(0m, discountAmount);
        Assert.Equal(100m, finalAmount);
    }

    #endregion
}

using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_ValidAmount_ShouldCreateMoney()
    {
        
        decimal amount = 100.50m;

        
        var money = new Money(amount);

        
        Assert.Equal(amount, money.Amount);
    }

    [Fact]
    public void Constructor_NegativeAmount_ShouldThrowDomainException()
    {
        var amount = -50.00m;
        
        var exception = Assert.Throws<DomainException>(() => new Money(amount));
        Assert.Equal("O valor não pode ser negativo.", exception.Message);
    }
    
    [Theory]
    [InlineData(100.123, 100.12)]
    [InlineData(100.456, 100.46)]
    [InlineData(100.789, 100.79)]
    public void Constructor_AmountWithMoreThanTwoDecimalPlaces_ShouldRoundToTwoDecimalPlaces(decimal input, decimal expected)
    {
        // Act
        var money = new Money(input);

        // Assert
        Assert.Equal(expected, money.Amount);
    }

    [Fact]
    public void Add_TwoMoneyObjects_ShouldReturnCorrectSum()
    {
        var money1 = new Money(50.25m);
        var money2 = new Money(75.75m);
        
        var result = money1.Add(money2);
        
        Assert.Equal(126.00m, result.Amount);
    }

    [Fact]
    public void Subtract_TwoMoneyObjects_ShouldReturnCorrectDifference()
    {
        var money1 = new Money(100.00m);
        var money2 = new Money(40.50m);
        var result = money1.Subtract(money2);
        Assert.Equal(59.50m, result.Amount);
    }

    [Fact]
    public void Subtract_ResultingInNegativeAmount_ShouldThrowDomainException()
    {
        var money1 = new Money(30.00m);
        var money2 = new Money(50.00m);
        var exception = Assert.Throws<DomainException>(() => money1.Subtract(money2));
        Assert.Equal("O valor não pode ser negativo.", exception.Message);
    }
    
    [Fact]
    public void Multiply_MoneyByScalar_ShouldReturnCorrectProduct()
    {
        var money = new Money(20.00m);
        var scalar = 2.5m;
        var result = money.Multiply(scalar);
        Assert.Equal(50.00m, result.Amount);
    }
    
    [Fact]
    public void Multiply_MoneyByNegativeScalar_ShouldThrowDomainException()
    {
        var money = new Money(20.00m);
        var scalar = -2.0m;
        var exception = Assert.Throws<DomainException>(() => money.Multiply(scalar));
        Assert.Equal("O valor não pode ser negativo.", exception.Message);
    }
    
    [Theory]
    [InlineData(100.50, true, false, false)]
    [InlineData(0, false, true, false)]
    public void Properties_ShouldReturnCorrectValues(decimal amount, bool isPositive, bool isZero, bool isNegative)
    {
        var money = new Money(amount);
        
        Assert.Equal(isPositive, money.IsPositive);
        Assert.Equal(isZero, money.IsZero);
        Assert.Equal(isNegative, money.IsNegative);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedBrazilianCurrency()
    {
        var money = new Money(1234.56m);
        var result = money.ToString();
        Assert.Equal("R$ 1.234,56", result);
    }
    
    [Fact]
    public void OperatorPlus_ShouldWorkCorrectly()
    {
        var money1 = new Money(30.00m);
        var money2 = new Money(70.00m);
        var result = money1 + money2;
        Assert.Equal(100.00m, result.Amount);
    }

    [Fact]
    public void OperatorMinus_ShouldWorkCorrectly()
    {
        var money1 = new Money(100.00m);
        var money2 = new Money(40.00m);
        var result = money1 - money2;
        Assert.Equal(60.00m, result.Amount);
    }

    [Fact]
    public void OperatorMultiply_ShouldWorkCorrectly()
    {
        var money = new Money(50.00m);
        var scalar = 3.0m;
        var result = money * scalar;
        Assert.Equal(150.00m, result.Amount);
    }
    
    [Fact]
    public void ImplicitConversion_FromDecimal_ShouldWorkCorrectly()
    {
        decimal amount = 100.00m;
        
        Money money = amount;
        
        Assert.Equal(amount, money.Amount);
    }
    
    [Fact]
    public void Equals_TwoMoneyObjectsWithSameAmount_ShouldBeEqual()
    {
        
        var money1 = new Money(100.50m);
        var money2 = new Money(100.50m);

        
        Assert.Equal(money1, money2);
        Assert.True(money1 == money2);
        Assert.False(money1 != money2);
    }

    [Fact]
    public void Equals_TwoMoneyObjectsWithDifferentAmounts_ShouldNotBeEqual()
    {
        
        var money1 = new Money(100.50m);
        var money2 = new Money(200.50m);

        // Act & Assert
        Assert.NotEqual(money1, money2);
        Assert.False(money1 == money2);
        Assert.True(money1 != money2);
    }

    [Fact]
    public void GetHashCode_TwoMoneyObjectsWithSameAmount_ShouldHaveSameHashCode()
    {
        
        var money1 = new Money(100.50m);
        var money2 = new Money(100.50m);

        
        Assert.Equal(money1.GetHashCode(), money2.GetHashCode());
    }
}
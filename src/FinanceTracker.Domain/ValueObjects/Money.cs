using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; }
    
    public Money(decimal amount)
    {
        if (amount < 0)
            throw new DomainException("O valor não pode ser negativo.");
        
        Amount = Math.Round(amount, 2);
    }

    public Money Add(Money other)
    {
        return new Money(Amount + other.Amount);
    }

    public Money Subtract(Money other)
    {
        return new Money(Amount - other.Amount);
    }

    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor);
    }
    
    public bool IsPositive => Amount > 0;
    public bool IsNegative => Amount < 0;
    public bool IsZero => Amount == 0;

    public override string ToString()
    {
        return $"R$ {Amount:N2}";
    }
    
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money left, decimal right) => left.Multiply(right);
    
    public static implicit operator Money(decimal amount) => new Money(amount);
    public static implicit operator decimal(Money money) => money.Amount;
};
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public string Description { get; private set; }
    public Money Amount { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public Guid CategoryId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Category Category { get; private set; }
    
    private Transaction(){}

    public Transaction(string description, Money amount, DateTime transactionDate, Guid categoryId)
    {
        ValidateDescription(description);
        ValidateTransactionDate(transactionDate);
        ValidateCategoryId(categoryId);
        
        Id = Guid.NewGuid();
        Description = description.Trim();
        Amount = amount;
        TransactionDate = transactionDate.Date;
        CategoryId = categoryId;
        CreatedAt = DateTime.UtcNow;
    }

    public Transaction(string description, decimal amount, DateTime transactionDate, Category category) : this(description, amount, transactionDate, category.Id)
    {
        Category = category;
    }

    public void UpdateDescription(string newDescription)
    {
        ValidateDescription(newDescription);
        Description = newDescription.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAmount(Money newAmount)
    {
        if (newAmount == null)
            throw new DomainException("O valor da transação não pode ser nulo.");
        
        Amount = newAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTransactionDate(DateTime newTransactionDate)
    {
        ValidateTransactionDate(newTransactionDate);
        TransactionDate = newTransactionDate.Date;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCategory(Category newCategory)
    {
        if (newCategory == null)
            throw new DomainException("A categoria não pode ser nula.");
        
        CategoryId = newCategory.Id;
        Category = newCategory;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsIncome => Category?.IsIncomeCategory ?? false;
    public bool IsExpense => Category?.IsExpenseCategory ?? false;
    public TransactionType TransactionType => Category?.TransactionType ?? TransactionType.Expense;
    public bool IsFromCurrentMonth => TransactionDate.Month == DateTime.Now.Month &&
                                      TransactionDate.Year == DateTime.Now.Year;
    public bool IsFromToday => TransactionDate.Date == DateTime.Today;
    
    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("A descrição da transação é obrigatória.");

        if (description.Trim().Length < 3)
            throw new DomainException("A descrição deve ter pelo menos 3 caracteres.");

        if (description.Trim().Length > 200)
            throw new DomainException("A descrição não pode exceder 200 caracteres.");
    }
    
    private static void ValidateTransactionDate(DateTime transactionDate)
    {
        if (transactionDate == default)
            throw new DomainException("A data da transação é obrigatória.");
        
        if (transactionDate.Date > DateTime.Today)
            throw new DomainException("A data da transação não pode ser futura.");
        
        var minimumDate = DateTime.Today.AddYears(-5);
        if (transactionDate.Date < minimumDate)
            throw new DomainException($"A data da transação não pode ser anterior a {minimumDate:dd/MM/yyyy}.");
    }

    public static void ValidateCategoryId(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            throw new DomainException("A categoria da transação é obrigatória.");
        }
    }

    public bool IsCategoryCompatible()
    {
        if (Category == null) return true;

        return Amount.IsPositive;

    }

    public override string ToString()
    {
        return $"{TransactionDate:dd/MM/yyyy} - {Description}: {Amount} ({Category?.DisplayName ?? "Sem categoria"})";
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Transaction other) return false;
        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
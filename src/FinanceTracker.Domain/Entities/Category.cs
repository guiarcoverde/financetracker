using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public CategoryType CategoryType { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private Category(){}

    public Category(string name, CategoryType categoryType)
    {
        ValidateName(name);
        
        Id = Guid.NewGuid();
        Name = name.Trim();
        CategoryType = categoryType;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string newName)
    {
        ValidateName(newName);
        Name = newName.Trim();
    }

    public void UpdateCategoryType(CategoryType newCategoryType)
    {
        CategoryType = newCategoryType;
    }

    public bool IsExpenseCategory => CategoryType.IsExpenseCategory();
    public bool IsIncomeCategory => CategoryType.IsIncomeCategory();
    public string DisplayName => CategoryType.GetDisplayName();
    
    public TransactionType TransactionType => CategoryType.GetTransactionType();

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome da categoria é obrigatório.");

        if (name.Trim().Length < 2)
            throw new DomainException("O nome da categoria deve ter pelo menos 2 caracteres.");

        if (name.Trim().Length > 50)
            throw new DomainException("O nome da categoria não pode exceder 50 caracteres.");
    }

    public override string ToString()
    {
        return $"{Name} ({DisplayName})";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Category other) return false;
        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
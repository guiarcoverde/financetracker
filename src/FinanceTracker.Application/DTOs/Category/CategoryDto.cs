using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.DTOs.Category;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryType CategoryType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsExpenseCategory { get; set; }
    public bool IsIncomeCategory { get; set; }
    public TransactionType TransactionType { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public int TransactionCount { get; set; }
}
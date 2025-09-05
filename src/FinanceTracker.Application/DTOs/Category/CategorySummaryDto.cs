using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.DTOs.Category;

public class CategorySummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public CategoryType CategoryType { get; set; }
    public TransactionType TransactionType { get; set; }
}
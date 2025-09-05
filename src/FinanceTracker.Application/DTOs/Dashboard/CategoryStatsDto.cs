using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.DTOs.Dashboard;

public class CategoryStatsDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public CategoryType CategoryType { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal TotalAmount { get; set; }
    public string FormattedAmount { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal Percentage { get; set; }
}
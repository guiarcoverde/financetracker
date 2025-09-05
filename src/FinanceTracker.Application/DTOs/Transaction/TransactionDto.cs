using FinanceTracker.Application.DTOs.Category;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.DTOs.Transaction;

public class TransactionDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string FormattedAmount { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string FormattedDate { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public CategorySummaryDto Category { get; set; } = new();
    public bool IsExpense { get; set; }
    public bool IsIncome { get; set; }
    public TransactionType TransactionType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
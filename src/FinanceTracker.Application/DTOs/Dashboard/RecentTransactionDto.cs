using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.DTOs.Dashboard;

public class RecentTransactionDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string FormattedAmount { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string RelativeDate { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
}
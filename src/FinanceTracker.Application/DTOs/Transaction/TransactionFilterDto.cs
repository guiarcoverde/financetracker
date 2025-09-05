using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.DTOs.Transaction;

public class TransactionFilterDto
{
    public Guid? CategoryId { get; set; }
    public TransactionType? TransactionType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? SearchTerm { get; set; }
    
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
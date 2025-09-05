namespace FinanceTracker.Application.DTOs.Transaction;

public class UpdateTransactionDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public Guid CategoryId { get; set; }
}
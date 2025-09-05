namespace FinanceTracker.Application.DTOs.Dashboard;

public class MonthlyStatsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance { get; set; }
    public string FormattedTotalIncome { get; set; } = string.Empty;
    public string FormattedTotalExpenses { get; set; } = string.Empty;
    public string FormattedBalance { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    
}
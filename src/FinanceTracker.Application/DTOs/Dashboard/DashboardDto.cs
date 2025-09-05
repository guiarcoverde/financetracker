namespace FinanceTracker.Application.DTOs.Dashboard;

public class DashboardDto
{
    public FinancialSummaryDto CurrentMonth { get; set; } = new();
    public FinancialSummaryDto LastMonth { get; set; } = new();
    public FinancialSummaryDto CurrentYear { get; set; } = new();
    public IEnumerable<RecentTransactionDto> RecentTransactions { get; set; } = new List<RecentTransactionDto>();
    public IEnumerable<CategoryStatsDto> TopCategories { get; set; } = new List<CategoryStatsDto>();
    public IEnumerable<MonthlyStatsDto> MonthlyTrend { get; set; } = new List<MonthlyStatsDto>();    
}
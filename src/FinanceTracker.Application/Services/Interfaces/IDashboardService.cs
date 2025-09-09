using FinanceTracker.Application.DTOs.Dashboard;

namespace FinanceTracker.Application.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync();
    Task<DashboardDto> GetDashboardForPeriodAsync(DateTime startDate, DateTime endDate);
    
    Task<FinancialSummaryDto> GetCurrentMonthSummaryAsync();
    Task<FinancialSummaryDto> GetLastMonthSummaryAsync();
    Task<FinancialSummaryDto> GetCurrentYearSummaryAsync();
    Task<FinancialSummaryDto> GetSummaryForPeriodAsync(DateTime startDate, DateTime endDate);
    
    Task<IEnumerable<FinancialSummaryDto>> GetCategoryStatsAsync();
    Task<IEnumerable<FinancialSummaryDto>> GetCategoryStatsForPeriodAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<MonthlyStatsDto>> GetTopCategoriesByExpenseAsync(int limt = 5);
    Task<IEnumerable<MonthlyStatsDto>> GetTopCategoriesByIncomeAsync(int limt = 5);
    
    Task<IEnumerable<MonthlyStatsDto>> GetMonthlyTrendAsync(int months = 12);
    Task<IEnumerable<MonthlyStatsDto>> GetYearlyTrendAsync(int years = 3);
    
    Task<IEnumerable<RecentTransactionDto>> GetRecentTransactionsAsync(int limit = 10);
    Task<IEnumerable<RecentTransactionDto>> GetRecentTransactionsByCategoryAsync(Guid categoryId, int limit = 5);

    Task<IEnumerable<ComparisonStatsDto>> GetMonthOverMonthComparisonAsync();
    Task<IEnumerable<ComparisonStatsDto>> GetYearOverYearComparisonAsync();

    Task<ProjectionDto> GetMonthlyProjectionAsync();
    Task<ProjectionDto> GetBudgetAnalysisAsync();

}
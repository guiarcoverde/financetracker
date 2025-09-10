using System.Collections;
using System.Globalization;
using FinanceTracker.Application.DTOs.Dashboard;
using FinanceTracker.Application.Services.Interfaces;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.Services.Implementations;

public class DashboardService(IUnitOfWork unitOfWork, 
    ITransactionService transactionService, 
    ICategoryService categoryService) : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly ICategoryService _categoryService = categoryService;
    
    public async Task<DashboardDto> GetDashboardAsync()
    {
        var currentMonth = await GetCurrentMonthSummaryAsync();
        var lastMonth = await GetLastMonthSummaryAsync();
        var currentYear = await GetCurrentYearSummaryAsync();
        var recentTransactions = await GetRecentTransactionsAsync(10);
        var topCategories = await GetTopCategoriesByExpenseAsync(5);
        var monthlyTrend = await GetMonthlyTrendAsync(6);

        return new DashboardDto
        {
            CurrentMonth = currentMonth,
            LastMonth = lastMonth,
            CurrentYear = currentYear,
            RecentTransactions = recentTransactions.ToList(),
            TopCategories = topCategories,
            MonthlyTrend = monthlyTrend
        };
    }

    public async Task<DashboardDto> GetDashboardForPeriodAsync(DateTime startDate, DateTime endDate)
    {
        ValidateDateRange(startDate, endDate);
        
        var periodSummary = await GetSummaryForPeriodAsync(startDate, endDate);
        var recentTransactions = await GetRecentTransactionsForPeriodAsync(startDate, endDate, 10);
        var categoryStats = await GetCategoryStatsForPeriodAsync(startDate, endDate);

        return new DashboardDto
        {
            CurrentMonth = periodSummary,
            LastMonth = new FinancialSummaryDto(),
            CurrentYear = new FinancialSummaryDto(),
            RecentTransactions = recentTransactions,
            TopCategories = categoryStats,
            MonthlyTrend = new List<MonthlyStatsDto>()
        };
    }

    public async Task<FinancialSummaryDto> GetCurrentMonthSummaryAsync()
    {
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        
        return await GetSummaryForPeriodAsync(startOfMonth, endOfMonth);
    }

    public async Task<FinancialSummaryDto> GetLastMonthSummaryAsync()
    {
        var now = DateTime.Now;
        var startOfLastMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
        var endOfLastMonth = startOfLastMonth.AddMonths(1).AddDays(-1);
        return await GetSummaryForPeriodAsync(startOfLastMonth, endOfLastMonth);
    }

    public Task<FinancialSummaryDto> GetCurrentYearSummaryAsync()
    {
        var now = DateTime.Now;
        var startOfYear = new DateTime(now.Year, 1, 1);
        var endOfYear = new DateTime(now.Year, 12, 31);
        return GetSummaryForPeriodAsync(startOfYear, endOfYear);
    }

    public async Task<FinancialSummaryDto> GetSummaryForPeriodAsync(DateTime startDate, DateTime endDate)
    {
        var totalIncome = 
            await _unitOfWork.Transactions.GetTotalByTypeAndDateRangeAsync(TransactionType.Income, startDate, endDate);
        var totalExpense = 
            await _unitOfWork.Transactions.GetTotalByTypeAndDateRangeAsync(TransactionType.Expense, startDate, endDate);
        var balance = totalIncome - totalExpense;
        
        var transactions = await _unitOfWork.Transactions.GetByDateRangeAsync(startDate, endDate);
        var transactionCount = transactions.Count();

        return new FinancialSummaryDto
        {
            Balance = balance,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpense,
            FormattedTotalIncome = FormatMoney(totalIncome),
            FormattedTotalExpenses = FormatMoney(totalExpense),
            TransactionCount = transactionCount,
            PeriodStart = startDate,
            PeriodEnd = endDate
        };
    }

    public Task<IEnumerable<CategoryStatsDto>> GetCategoryStatsAsync()
    {
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        return GetCategoryStatsForPeriodAsync(startOfMonth, endOfMonth);
    }

    public async Task<IEnumerable<CategoryStatsDto>> GetCategoryStatsForPeriodAsync(DateTime startDate, DateTime endDate)
    {
        ValidateDateRange(startDate, endDate);

        var categories = await _unitOfWork.Categories.GetAllAsync();
        var transactions = await _unitOfWork.Transactions.GetByDateRangeAsync(startDate, endDate);

        var categoryStats = new List<CategoryStatsDto>();
        decimal totalAmount = transactions.Sum(t => t.Amount.Amount);

        foreach (var category in categories)
        {
            var categoryTransactions = 
                transactions.Where(t => t.CategoryId == category.Id).ToList();

            if (!categoryTransactions.Any()) continue;
            
            var categoryTotal = categoryTransactions.Sum(t => t.Amount.Amount);
            var percentage = totalAmount > 0 ? (categoryTotal / totalAmount) * 100 : 0;
            
            categoryStats.Add(new CategoryStatsDto
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                CategoryType = category.CategoryType,
                TransactionType = category.TransactionType,
                TotalAmount = categoryTotal,
                FormattedAmount = FormatMoney(categoryTotal),
                TransactionCount = categoryTransactions.Count,
                Percentage = Math.Round(percentage, 2)
            });
        }
        
        return categoryStats.OrderByDescending(c => c.TotalAmount).ToList();
    }

    public async Task<IEnumerable<CategoryStatsDto>> GetTopCategoriesByExpenseAsync(int limt = 5)
    {
        var stats = await GetCategoryStatsAsync();
        return stats
            .Where(s => s.TransactionType == TransactionType.Expense)
            .OrderByDescending(s => s.TotalAmount)
            .Take(limt)
            .ToList();
    }

    public async Task<IEnumerable<CategoryStatsDto>> GetTopCategoriesByIncomeAsync(int limt = 5)
    {
        var stats = await GetCategoryStatsAsync();
        return stats
            .Where(s => s.TransactionType == TransactionType.Income)
            .OrderByDescending(s => s.TotalAmount)
            .Take(limt)
            .ToList();
    }

    public async Task<IEnumerable<MonthlyStatsDto>> GetMonthlyTrendAsync(int months = 12)
    {
        if(months is <= 0 or > 24)
            throw new DomainException("Número de meses deve estar entre 1 e 24.");
        
        var monthlyStats = new List<MonthlyStatsDto>();
        var currentDate = DateTime.Now;

        for (int i = months - 1; i >= 0; i--)
        {
            var targetDate = currentDate.AddMonths(-i);
            var startOfMonth = new DateTime(targetDate.Year, targetDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            
            var summary = await GetSummaryForPeriodAsync(startOfMonth, endOfMonth);
            
            monthlyStats.Add(new MonthlyStatsDto
            {
                Year = targetDate.Year,
                Month = targetDate.Month,
                MonthName = targetDate.ToString("MMMM yyyy", new CultureInfo("pt-BR")),
                TotalIncome = summary.TotalIncome,
                TotalExpenses = summary.TotalExpenses,
                Balance = summary.Balance,
                FormattedTotalIncome = summary.FormattedTotalIncome,
                FormattedTotalExpenses = summary.FormattedTotalExpenses,
                FormattedBalance = summary.FormattedBalance,
                TransactionCount = summary.TransactionCount,
            });
        }
        
        return monthlyStats;
    }

    public async Task<IEnumerable<MonthlyStatsDto>> GetYearlyTrendAsync(int years = 3)
    {
        if (years is <= 0 or > 10)
            throw new DomainException("Número de anos deve estar entre 1 e 10.");
        
        var yearlyStats = new List<MonthlyStatsDto>();
        var currentYear = DateTime.Now.Year;
        
        for (int i = years - 1; i >= 0; i--)
        {
            var targetYear = currentYear ;
            var startOfYear = new DateTime(targetYear, 1, 1);
            var endOfYear = new DateTime(targetYear, 12, 31);
            
            var summary = await GetSummaryForPeriodAsync(startOfYear, endOfYear);
            
            yearlyStats.Add(new MonthlyStatsDto
            {
                Year = targetYear,
                Month = 0,
                MonthName = targetYear.ToString(),
                TotalIncome = summary.TotalIncome,
                TotalExpenses = summary.TotalExpenses,
                Balance = summary.Balance,
                FormattedTotalIncome = summary.FormattedTotalIncome,
                FormattedTotalExpenses = summary.FormattedTotalExpenses,
                FormattedBalance = summary.FormattedBalance,
                TransactionCount = summary.TransactionCount,
            });
        }
        
        return yearlyStats;
    }

    public async Task<IEnumerable<RecentTransactionDto>> GetRecentTransactionsAsync(int limit = 10)
    {
        if (limit is <= 0 or > 50)
            throw new DomainException("Limite deve estar entre 1 e 50.");

        var transactions = await _unitOfWork.Transactions.GetAllWithCategoriesAsync();
        
        return transactions
            .OrderByDescending(t => t.CreatedAt)
            .Take(limit)
            .Select(MapToRecentTransactionDto)
            .ToList();
    }

    public async Task<IEnumerable<RecentTransactionDto>> GetRecentTransactionsByCategoryAsync(Guid categoryId, int limit = 5)
    {
        if (limit is <= 0 or > 20)
            throw new DomainException("Limite deve estar entre 1 e 20.");
        
        var transactions = await _unitOfWork.Transactions.GetByCategoryIdAsync(categoryId);

        return transactions
            .OrderByDescending(t => t.CreatedAt)
            .Take(limit)
            .Select(MapToRecentTransactionDto)
            .ToList();
    }
    
    public async Task<ComparisonStatsDto> GetMonthOverMonthComparisonAsync()
    {
        var currentMonth = await GetCurrentMonthSummaryAsync();
        var lastMonth = await GetLastMonthSummaryAsync();
        
        return CreateComparisonStats(currentMonth, lastMonth, "mês atual vs mês anterior");
    }

    public async Task<ComparisonStatsDto> GetYearOverYearComparisonAsync()
    {
        var currentYear = await GetCurrentYearSummaryAsync();
        var lastYear = DateTime.Now.Year - 1;
        var startOfLastYear = new DateTime(lastYear, 1, 1);
        var endOfLastYear = new DateTime(lastYear, 12, 31);
        var previousYear = await GetSummaryForPeriodAsync(startOfLastYear, endOfLastYear);
        
        return CreateComparisonStats(currentYear, previousYear, $"{DateTime.Now.Year} vs {lastYear}");
    }

    public async Task<ProjectionDto> GetMonthlyProjectionAsync()
    {
        var monthlyTrend = await GetMonthlyTrendAsync(3);
        var trends = monthlyTrend.ToList();

        if (!trends.Any())
        {
            return new ProjectionDto
            {
                ProjectionDate = DateTime.Now.AddMonths(1),
                ProjectedIncome = 0,
                ProjectedExpenses = 0,
                ProjectedBalance = 0,
                ProjectionMethod = "Dados insuficientes",
                DataPointsUsed = 0,
                ConfidenceLevel = 0
            };
        }
        
        var avgIncome = trends.Average(t => t.TotalIncome);
        var avgExpenses = trends.Average(t => t.TotalExpenses);
        var projectedBalance = avgIncome - avgExpenses;
        
        var nextMonth = DateTime.Now.AddMonths(1);

        return new ProjectionDto
        {
            ProjectionDate = nextMonth,
            ProjectedIncome = Math.Round(avgIncome, 2),
            ProjectedExpenses = Math.Round(avgExpenses, 2),
            ProjectedBalance = Math.Round(projectedBalance, 2),
            FormattedProjectedIncome = FormatMoney(avgIncome),
            FormattedProjectedExpenses = FormatMoney(avgExpenses),
            FormattedProjectedBalance = FormatMoney(projectedBalance),
            ProjectionMethod = $"Média dos últimos {trends.Count} meses",
            DataPointsUsed = trends.Count,
            ConfidenceLevel = trends.Count >= 3 ? 75 : 50
        };

    }

    public async Task<BudgetAnalysisDto> GetBudgetAnalysisAsync()
    {
        await Task.CompletedTask;

        return new BudgetAnalysisDto
        {
            BudgetedIncome = 0,
            ActualIncome = 0,
            BudgetedExpenses = 0,
            ActualExpenses = 0,
            CategoryAnalysis = new List<CategoryBudgetDto>()
        };
    }
    
    private async Task<IEnumerable<RecentTransactionDto>> GetRecentTransactionsForPeriodAsync(DateTime startDate, DateTime endDate, int limit)
    {
        var transactions = await _unitOfWork.Transactions.GetByDateRangeAsync(startDate, endDate);
        
        return transactions
            .OrderByDescending(t => t.CreatedAt)
            .Take(limit)
            .Select(MapToRecentTransactionDto)
            .ToList();
    }

    private static RecentTransactionDto MapToRecentTransactionDto(Transaction transaction)
        => new RecentTransactionDto
        {
            Id = transaction.Id,
            Description = transaction.Description,
            Amount = transaction.Amount.Amount,
            FormattedAmount = transaction.Amount.ToString(),
            TransactionDate = transaction.TransactionDate,
            RelativeDate = GetRelativeDate(transaction.TransactionDate),
            CategoryName = transaction.Category?.Name ?? "Sem categoria",
            TransactionType = transaction.TransactionType,
        };

    private static string GetRelativeDate(DateTime date)
    {
        var today = DateTime.Today;
        var daysDiff = (today - date.Date).Days;

        return daysDiff switch
        {
            0 => "Hoje",
            1 => "Ontem",
            <= 7 => $"{daysDiff} dias atrás",
            <= 30 => $"{daysDiff / 7} semana(s) atrás",
            _ => date.ToString("dd/MM/yyyy")
        };
    }

    private static ComparisonStatsDto CreateComparisonStats(
        FinancialSummaryDto current,
        FinancialSummaryDto previous,
        string periodDescription)
    {
        var incomeVariation = current.TotalIncome - previous.TotalIncome;
        var expenseVariation = current.TotalExpenses - previous.TotalExpenses;
        var balanceVariation = current.Balance - previous.Balance;

        return new ComparisonStatsDto
        {
            CurrentPeriod = current,
            PreviousPeriod = previous,
            IncomeVariationAmount = incomeVariation,
            ExpenseVariationAmount = expenseVariation,
            BalanceVariationAmount = balanceVariation,
            IncomeVariationPercentage = CalculatePercentageVariation(current.TotalIncome, previous.TotalIncome),
            ExpenseVariationPercentage = CalculatePercentageVariation(current.TotalExpenses, previous.TotalExpenses),
            BalanceVariationPercentage = CalculatePercentageVariation(current.Balance, previous.Balance),
            PeriodDescription = periodDescription
        };
    }
    
    private static decimal CalculatePercentageVariation(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? 0 : 100;
        return Math.Round(((current - previous) / Math.Abs(previous)) * 100, 2);
    }
    
    private static string FormatMoney(decimal amount) => $"R$ {amount:N2}";

    private static void ValidateDateRange(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            throw new DomainException("A data inicial não pode ser maior que a data final.");

        if (startDate > DateTime.Today)
            throw new DomainException("A data inicial não pode ser futura.");
    }
}
using FinanceTracker.Application.DTOs.Dashboard;
using FinanceTracker.Application.Services.Implementations;
using FinanceTracker.Application.Services.Interfaces;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Domain.ValueObjects;
using Moq;

namespace FinanceTracker.Application.Tests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITransactionService> _transactionServiceMock;
    private readonly Mock<ICategoryService> _categoryServiceMock;
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly DashboardService _dashboardService;

    private readonly Category _expenseCategory;
    private readonly Category _incomeCategory;
    private readonly List<Transaction> _sampleTransactions;

    public DashboardServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _transactionServiceMock = new Mock<ITransactionService>();
        _categoryServiceMock = new Mock<ICategoryService>();
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();

        // Configurar UnitOfWork
        _unitOfWorkMock.Setup(uow => uow.Transactions).Returns(_transactionRepositoryMock.Object);
        _unitOfWorkMock.Setup(uow => uow.Categories).Returns(_categoryRepositoryMock.Object);

        _dashboardService = new DashboardService(
            _unitOfWorkMock.Object,
            _transactionServiceMock.Object,
            _categoryServiceMock.Object);

        // Criar dados de teste
        _expenseCategory = new Category("Alimentação", CategoryType.Food);
        _incomeCategory = new Category("Salário", CategoryType.Salary);
        SetCategoryId(_expenseCategory, Guid.NewGuid());
        SetCategoryId(_incomeCategory, Guid.NewGuid());

        _sampleTransactions = CreateSampleTransactions();
    }

    [Fact]
    public async Task GetCurrentMonthSummaryAsync_ShouldReturnCorrectSummary()
    {
        // Arrange
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var totalIncome = 3000.00m;
        var totalExpenses = 1500.00m;
        var transactionCount = 10;

        _transactionRepositoryMock
            .Setup(repo => repo.GetTotalByTypeAndDateRangeAsync(TransactionType.Income, startOfMonth, endOfMonth))
            .ReturnsAsync(totalIncome);

        _transactionRepositoryMock
            .Setup(repo => repo.GetTotalByTypeAndDateRangeAsync(TransactionType.Expense, startOfMonth, endOfMonth))
            .ReturnsAsync(totalExpenses);

        _transactionRepositoryMock
            .Setup(repo => repo.GetByDateRangeAsync(startOfMonth, endOfMonth))
            .ReturnsAsync(_sampleTransactions.Take(transactionCount));

        // Act
        var result = await _dashboardService.GetCurrentMonthSummaryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(totalIncome, result.TotalIncome);
        Assert.Equal(totalExpenses, result.TotalExpenses);
        Assert.Equal(totalIncome - totalExpenses, result.Balance);
        Assert.Equal("R$ 3.000,00", result.FormattedTotalIncome);
        Assert.Equal("R$ 1.500,00", result.FormattedTotalExpenses);
        Assert.Equal("R$ 1.500,00", result.FormattedBalance);
        Assert.Equal(transactionCount, result.TransactionCount);
        Assert.Equal(startOfMonth, result.PeriodStart);
        Assert.Equal(endOfMonth, result.PeriodEnd);
    }

    [Fact]
    public async Task GetLastMonthSummaryAsync_ShouldReturnLastMonthData()
    {
        // Arrange
        var now = DateTime.Now;
        var startOfLastMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
        var endOfLastMonth = startOfLastMonth.AddMonths(1).AddDays(-1);

        var totalIncome = 2800.00m;
        var totalExpenses = 1200.00m;

        _transactionRepositoryMock
            .Setup(repo => repo.GetTotalByTypeAndDateRangeAsync(TransactionType.Income, startOfLastMonth, endOfLastMonth))
            .ReturnsAsync(totalIncome);

        _transactionRepositoryMock
            .Setup(repo => repo.GetTotalByTypeAndDateRangeAsync(TransactionType.Expense, startOfLastMonth, endOfLastMonth))
            .ReturnsAsync(totalExpenses);

        _transactionRepositoryMock
            .Setup(repo => repo.GetByDateRangeAsync(startOfLastMonth, endOfLastMonth))
            .ReturnsAsync(_sampleTransactions.Take(8));

        // Act
        var result = await _dashboardService.GetLastMonthSummaryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(totalIncome, result.TotalIncome);
        Assert.Equal(totalExpenses, result.TotalExpenses);
        Assert.Equal(totalIncome - totalExpenses, result.Balance);
        Assert.Equal(8, result.TransactionCount);
    }

    [Fact]
    public async Task GetSummaryForPeriodAsync_ValidPeriod_ShouldReturnCorrectSummary()
    {
        // Arrange
        var startDate = new DateTime(2024, 3, 1);
        var endDate = new DateTime(2024, 3, 31);
        var totalIncome = 2500.00m;
        var totalExpenses = 1000.00m;

        _transactionRepositoryMock
            .Setup(repo => repo.GetTotalByTypeAndDateRangeAsync(TransactionType.Income, startDate, endDate))
            .ReturnsAsync(totalIncome);

        _transactionRepositoryMock
            .Setup(repo => repo.GetTotalByTypeAndDateRangeAsync(TransactionType.Expense, startDate, endDate))
            .ReturnsAsync(totalExpenses);

        _transactionRepositoryMock
            .Setup(repo => repo.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(_sampleTransactions.Take(5));

        // Act
        var result = await _dashboardService.GetSummaryForPeriodAsync(startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(totalIncome, result.TotalIncome);
        Assert.Equal(totalExpenses, result.TotalExpenses);
        Assert.Equal(1500.00m, result.Balance);
        Assert.Equal(startDate, result.PeriodStart);
        Assert.Equal(endDate, result.PeriodEnd);
    }

    [Fact]
    public async Task GetCategoryStatsForPeriodAsync_ShouldReturnStatsWithPercentages()
    {
        // Arrange
        var startDate = new DateTime(2024, 3, 1);
        var endDate = new DateTime(2024, 3, 31);

        var categories = new List<Category> { _expenseCategory, _incomeCategory };
        var transactions = new List<Transaction>
        {
            new("Compras", new Money(300m), startDate, _expenseCategory),
            new("Compras 2", new Money(200m), startDate, _expenseCategory),
            new("Salário", new Money(2000m), startDate, _incomeCategory)
        };

        _categoryRepositoryMock
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(categories);

        _transactionRepositoryMock
            .Setup(repo => repo.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(transactions);

        // Act
        var result = await _dashboardService.GetCategoryStatsForPeriodAsync(startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());

        var stats = result.OrderByDescending(s => s.TotalAmount).ToList();
        
        // Primeira categoria (maior valor)
        Assert.Equal(_incomeCategory.Id, stats[0].CategoryId);
        Assert.Equal(2000m, stats[0].TotalAmount);
        Assert.Equal(1, stats[0].TransactionCount);
        Assert.Equal(80m, stats[0].Percentage); // 2000 de 2500 total = 80%

        // Segunda categoria
        Assert.Equal(_expenseCategory.Id, stats[1].CategoryId);
        Assert.Equal(500m, stats[1].TotalAmount);
        Assert.Equal(2, stats[1].TransactionCount);
        Assert.Equal(20m, stats[1].Percentage); // 500 de 2500 total = 20%
    }

    [Fact]
    public async Task GetTopCategoriesByExpenseAsync_ShouldReturnOnlyExpenseCategories()
    {
        // Arrange
        var limit = 3;
        var mockStats = new List<CategoryStatsDto>
        {
            new() { CategoryId = _expenseCategory.Id, CategoryName = "Alimentação", TransactionType = TransactionType.Expense, TotalAmount = 500m },
            new() { CategoryId = _incomeCategory.Id, CategoryName = "Salário", TransactionType = TransactionType.Income, TotalAmount = 3000m }
        };

        SetupCategoryStatsAsync(mockStats);

        // Act
        var result = await _dashboardService.GetTopCategoriesByExpenseAsync(limit);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.All(result, stat => Assert.Equal(TransactionType.Expense, stat.TransactionType));
    }

    [Fact]
    public async Task GetMonthlyTrendAsync_ShouldReturnTrendForSpecifiedMonths()
    {
        // Arrange
        var months = 3;
        var currentDate = DateTime.Now;

        // Configurar para cada mês
        for (int i = months - 1; i >= 0; i--)
        {
            var targetDate = currentDate.AddMonths(-i);
            var startOfMonth = new DateTime(targetDate.Year, targetDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var income = 3000m - (i * 200m); // Receita variável
            var expenses = 1500m + (i * 100m); // Despesa variável

            _transactionRepositoryMock
                .Setup(repo => repo.GetTotalByTypeAndDateRangeAsync(TransactionType.Income, startOfMonth, endOfMonth))
                .ReturnsAsync(income);

            _transactionRepositoryMock
                .Setup(repo => repo.GetTotalByTypeAndDateRangeAsync(TransactionType.Expense, startOfMonth, endOfMonth))
                .ReturnsAsync(expenses);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByDateRangeAsync(startOfMonth, endOfMonth))
                .ReturnsAsync(_sampleTransactions.Take(5));
        }

        // Act
        var result = await _dashboardService.GetMonthlyTrendAsync(months);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(months, result.Count());

        var trends = result.ToList();
        Assert.All(trends, trend => 
        {
            Assert.True(trend.TotalIncome > 0);
            Assert.True(trend.TotalExpenses > 0);
            Assert.Equal(5, trend.TransactionCount);
            Assert.NotEmpty(trend.MonthName);
        });
    }

    [Fact]
    public async Task GetRecentTransactionsAsync_ShouldReturnLimitedTransactions()
    {
        // Arrange
        var limit = 5;
        var recentTransactions = _sampleTransactions.Take(limit).ToList();

        _transactionRepositoryMock
            .Setup(repo => repo.GetAllWithCategoriesAsync())
            .ReturnsAsync(_sampleTransactions);

        // Act
        var result = await _dashboardService.GetRecentTransactionsAsync(limit);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(limit, result.Count());

        var transactions = result.ToList();
        Assert.All(transactions, transaction =>
        {
            Assert.NotEmpty(transaction.Description);
            Assert.True(transaction.Amount > 0);
            Assert.NotEmpty(transaction.FormattedAmount);
            Assert.NotEmpty(transaction.RelativeDate);
        });
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnCompleteDashboard()
    {
        // Arrange
        SetupFullDashboard();

        // Act
        var result = await _dashboardService.GetDashboardAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.CurrentMonth);
        Assert.NotNull(result.LastMonth);
        Assert.NotNull(result.CurrentYear);
        Assert.NotNull(result.RecentTransactions);
        Assert.NotNull(result.TopCategories);
        Assert.NotNull(result.MonthlyTrend);

        Assert.True(result.RecentTransactions.Count() <= 10);
        Assert.True(result.TopCategories.Count() <= 5);
        Assert.True(result.MonthlyTrend.Count() <= 6);
    }

    [Fact]
    public async Task GetMonthOverMonthComparisonAsync_ShouldReturnComparison()
    {
        // Arrange
        var currentMonth = new FinancialSummaryDto
        {
            TotalIncome = 3000m,
            TotalExpenses = 1500m,
            Balance = 1500m
        };

        var lastMonth = new FinancialSummaryDto
        {
            TotalIncome = 2800m,
            TotalExpenses = 1200m,
            Balance = 1600m
        };

        SetupMonthSummaries(currentMonth, lastMonth);

        // Act
        var result = await _dashboardService.GetMonthOverMonthComparisonAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(currentMonth.TotalIncome, result.CurrentPeriod.TotalIncome);
        Assert.Equal(lastMonth.TotalIncome, result.PreviousPeriod.TotalIncome);

        // Variações
        Assert.Equal(200m, result.IncomeVariationAmount); // 3000 - 2800
        Assert.Equal(300m, result.ExpenseVariationAmount); // 1500 - 1200
        Assert.Equal(-100m, result.BalanceVariationAmount); // 1500 - 1600

        // Percentuais (aproximados)
        Assert.True(result.IncomeVariationPercentage > 0); // Aumento
        Assert.True(result.ExpenseVariationPercentage > 0); // Aumento
        Assert.True(result.BalanceVariationPercentage < 0); // Diminuição

        // Indicadores de melhoria
        Assert.True(result.IncomeImproved); // Mais receita = melhoria
        Assert.False(result.ExpenseImproved); // Mais despesa = piora
        Assert.False(result.BalanceImproved); // Menos saldo = piora
    }

    [Fact]
    public async Task GetMonthlyProjectionAsync_ShouldReturnProjection()
    {
        // Arrange
        var monthlyTrends = new List<MonthlyStatsDto>
        {
            new() { TotalIncome = 2800m, TotalExpenses = 1200m, Balance = 1600m },
            new() { TotalIncome = 3000m, TotalExpenses = 1400m, Balance = 1600m },
            new() { TotalIncome = 3200m, TotalExpenses = 1500m, Balance = 1700m }
        };

        SetupMonthlyTrend(monthlyTrends);

        // Act
        var result = await _dashboardService.GetMonthlyProjectionAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ProjectedIncome > 0);
        Assert.True(result.ProjectedExpenses > 0);
        Assert.NotEmpty(result.FormattedProjectedIncome);
        Assert.NotEmpty(result.FormattedProjectedExpenses);
        Assert.NotEmpty(result.FormattedProjectedBalance);
        Assert.Equal(3, result.DataPointsUsed);
        Assert.Equal(75, result.ConfidenceLevel); // 3 pontos = 75% confiança
        Assert.Contains("Média dos últimos 3 meses", result.ProjectionMethod);
    }

    // Métodos auxiliares para setup dos mocks
    private void SetupCategoryStatsAsync(List<CategoryStatsDto> stats)
    {
        var categories = new List<Category> { _expenseCategory, _incomeCategory };
        var transactions = new List<Transaction>
        {
            new("Despesa", new Money(500m), DateTime.Now, _expenseCategory),
            new("Receita", new Money(3000m), DateTime.Now, _incomeCategory)
        };

        _categoryRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(categories);
        _transactionRepositoryMock.Setup(repo => repo.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(transactions);
    }

    private void SetupFullDashboard()
    {
        var summary = new FinancialSummaryDto { TotalIncome = 3000m, TotalExpenses = 1500m, Balance = 1500m };
        
        SetupMonthSummaries(summary, summary);
        SetupCategoryStatsAsync(new List<CategoryStatsDto>());
        
        _transactionRepositoryMock.Setup(repo => repo.GetAllWithCategoriesAsync())
            .ReturnsAsync(_sampleTransactions);
    }

    private void SetupMonthSummaries(FinancialSummaryDto current, FinancialSummaryDto last)
    {
        // Setup para mês atual
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        _transactionRepositoryMock.Setup(repo => repo.GetTotalByTypeAndDateRangeAsync(TransactionType.Income, startOfMonth, endOfMonth))
            .ReturnsAsync(current.TotalIncome);
        _transactionRepositoryMock.Setup(repo => repo.GetTotalByTypeAndDateRangeAsync(TransactionType.Expense, startOfMonth, endOfMonth))
            .ReturnsAsync(current.TotalExpenses);
        _transactionRepositoryMock.Setup(repo => repo.GetByDateRangeAsync(startOfMonth, endOfMonth))
            .ReturnsAsync(_sampleTransactions.Take(5));

        // Setup para mês anterior
        var startOfLastMonth = startOfMonth.AddMonths(-1);
        var endOfLastMonth = startOfLastMonth.AddMonths(1).AddDays(-1);

        _transactionRepositoryMock.Setup(repo => repo.GetTotalByTypeAndDateRangeAsync(TransactionType.Income, startOfLastMonth, endOfLastMonth))
            .ReturnsAsync(last.TotalIncome);
        _transactionRepositoryMock.Setup(repo => repo.GetTotalByTypeAndDateRangeAsync(TransactionType.Expense, startOfLastMonth, endOfLastMonth))
            .ReturnsAsync(last.TotalExpenses);
        _transactionRepositoryMock.Setup(repo => repo.GetByDateRangeAsync(startOfLastMonth, endOfLastMonth))
            .ReturnsAsync(_sampleTransactions.Take(3));
    }

    private void SetupMonthlyTrend(List<MonthlyStatsDto> trends)
    {
        var currentDate = DateTime.Now;
        for (int i = 0; i < trends.Count; i++)
        {
            var targetDate = currentDate.AddMonths(-(trends.Count - 1 - i));
            var startOfMonth = new DateTime(targetDate.Year, targetDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            _transactionRepositoryMock.Setup(repo => repo.GetTotalByTypeAndDateRangeAsync(TransactionType.Income, startOfMonth, endOfMonth))
                .ReturnsAsync(trends[i].TotalIncome);
            _transactionRepositoryMock.Setup(repo => repo.GetTotalByTypeAndDateRangeAsync(TransactionType.Expense, startOfMonth, endOfMonth))
                .ReturnsAsync(trends[i].TotalExpenses);
            _transactionRepositoryMock.Setup(repo => repo.GetByDateRangeAsync(startOfMonth, endOfMonth))
                .ReturnsAsync(_sampleTransactions.Take(5));
        }
    }

    private List<Transaction> CreateSampleTransactions()
    {
        return new List<Transaction>
        {
            new("Compras 1", new Money(150m), DateTime.Today, _expenseCategory),
            new("Compras 2", new Money(200m), DateTime.Today.AddDays(-1), _expenseCategory),
            new("Salário", new Money(3000m), DateTime.Today.AddDays(-2), _incomeCategory),
            new("Compras 3", new Money(75m), DateTime.Today.AddDays(-3), _expenseCategory),
            new("Freelance", new Money(500m), DateTime.Today.AddDays(-4), _incomeCategory)
        };
    }

    private static void SetCategoryId(Category category, Guid id)
    {
        var idProperty = typeof(Category).GetProperty("Id");
        idProperty?.SetValue(category, id);
    }
}
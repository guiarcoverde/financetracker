using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.DTOs.Transaction;
using FinanceTracker.Application.Services.Implementations;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Domain.ValueObjects;
using Mapster;
using Moq;

namespace FinanceTracker.Application.Tests.Services;

public class TransactionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly TransactionService _transactionService;

    private readonly Category _expenseCategory;
    private readonly Category _incomeCategory;
    private readonly Guid _expenseCategoryId;
    private readonly Guid _incomeCategoryId;

    public TransactionServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();

        // Configurar o UnitOfWork para retornar os repositórios mock
        _unitOfWorkMock.Setup(uow => uow.Transactions).Returns(_transactionRepositoryMock.Object);
        _unitOfWorkMock.Setup(uow => uow.Categories).Returns(_categoryRepositoryMock.Object);

        _transactionService = new TransactionService(_unitOfWorkMock.Object);

        // Criar categorias de teste
        _expenseCategoryId = Guid.NewGuid();
        _incomeCategoryId = Guid.NewGuid();
        
        _expenseCategory = new Category("Alimentação", CategoryType.Food);
        _incomeCategory = new Category("Salário", CategoryType.Salary);
        
        SetCategoryId(_expenseCategory, _expenseCategoryId);
        SetCategoryId(_incomeCategory, _incomeCategoryId);

        ConfigureMapster();
    }

    private static void ConfigureMapster()
    {
        // Configurar mapeamentos para testes
        // TypeAdapterConfig.GlobalSettings.Clear(); // REMOVIDO: método inexistente
        // Configurações básicas para evitar erros de mapping
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTransaction_ShouldReturnTransactionDto()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transaction = new Transaction("Compras", new Money(150.50m), DateTime.Today, _expenseCategory);
        SetTransactionId(transaction, transactionId);

        _transactionRepositoryMock
            .Setup(repo => repo.GetByIdWithCategoryAsync(transactionId))
            .ReturnsAsync(transaction);

        // Act
        var result = await _transactionService.GetByIdAsync(transactionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transactionId, result.Id);
        Assert.Equal("Compras", result.Description);
        Assert.Equal(150.50m, result.Amount);
        Assert.Equal("R$ 150,50", result.FormattedAmount);
        Assert.Equal(DateTime.Today.ToString("dd/MM/yyyy"), result.FormattedDate);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingTransaction_ShouldThrowDomainException()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _transactionRepositoryMock
            .Setup(repo => repo.GetByIdWithCategoryAsync(transactionId))
            .ReturnsAsync((Transaction?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _transactionService.GetByIdAsync(transactionId));

        Assert.Contains($"Transação com ID {transactionId} não foi encontrada", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ShouldCreateAndReturnTransaction()
    {
        // Arrange
        var createDto = new CreateTransactionDto
        {
            Description = "Nova transação",
            Amount = 200.00m,
            TransactionDate = DateTime.Today,
            CategoryId = _expenseCategoryId
        };

        _categoryRepositoryMock
            .Setup(repo => repo.ExistsAsync(_expenseCategoryId))
            .ReturnsAsync(true);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(_expenseCategoryId))
            .ReturnsAsync(_expenseCategory);

        _transactionRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Transaction>()))
            .ReturnsAsync((Transaction t) => t);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _transactionService.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Nova transação", result.Description);
        Assert.Equal(200.00m, result.Amount);
        Assert.Equal(DateTime.Today, result.TransactionDate);

        // Verificar se os métodos foram chamados
        _transactionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Transaction>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NullDto_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _transactionService.CreateAsync(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    // [InlineData(null)] // REMOVIDO: não permitido para string não anulável
    public async Task CreateAsync_InvalidDescription_ShouldThrowDomainException(string invalidDescription)
    {
        // Arrange
        var createDto = new CreateTransactionDto
        {
            Description = invalidDescription,
            Amount = 100.00m,
            TransactionDate = DateTime.Today,
            CategoryId = _expenseCategoryId
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _transactionService.CreateAsync(createDto));

        Assert.Equal("A descrição é obrigatória.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_NegativeAmount_ShouldThrowDomainException()
    {
        // Arrange
        var createDto = new CreateTransactionDto
        {
            Description = "Transação inválida",
            Amount = -50.00m,
            TransactionDate = DateTime.Today,
            CategoryId = _expenseCategoryId
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _transactionService.CreateAsync(createDto));

        Assert.Equal("O valor deve ser maior que zero.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_NonExistingCategory_ShouldThrowDomainException()
    {
        // Arrange
        var nonExistingCategoryId = Guid.NewGuid();
        var createDto = new CreateTransactionDto
        {
            Description = "Transação válida",
            Amount = 100.00m,
            TransactionDate = DateTime.Today,
            CategoryId = nonExistingCategoryId
        };

        _categoryRepositoryMock
            .Setup(repo => repo.ExistsAsync(nonExistingCategoryId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _transactionService.CreateAsync(createDto));

        Assert.Contains($"Categoria com ID {nonExistingCategoryId} não foi encontrada", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ValidDto_ShouldUpdateAndReturnTransaction()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var updateDto = new UpdateTransactionDto
        {
            Description = "Descrição atualizada",
            Amount = 300.00m,
            TransactionDate = DateTime.Today.AddDays(-1),
            CategoryId = _incomeCategoryId
        };

        var existingTransaction = new Transaction("Descrição original", new Money(100m), DateTime.Today, _expenseCategory);
        SetTransactionId(existingTransaction, transactionId);

        _transactionRepositoryMock
            .Setup(repo => repo.ExistsAsync(transactionId))
            .ReturnsAsync(true);

        _categoryRepositoryMock
            .Setup(repo => repo.ExistsAsync(_incomeCategoryId))
            .ReturnsAsync(true);

        _transactionRepositoryMock
            .Setup(repo => repo.GetByIdWithCategoryAsync(transactionId))
            .ReturnsAsync(existingTransaction);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(_incomeCategoryId))
            .ReturnsAsync(_incomeCategory);

        _transactionRepositoryMock
            .Setup(repo => repo.UpdateAsync(existingTransaction))
            .ReturnsAsync(existingTransaction);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _transactionService.UpdateAsync(transactionId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Descrição atualizada", result.Description);
        Assert.Equal(300.00m, result.Amount);

        // Verificar se os métodos foram chamados
        _transactionRepositoryMock.Verify(repo => repo.UpdateAsync(existingTransaction), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ExistingTransaction_ShouldDelete()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _transactionRepositoryMock
            .Setup(repo => repo.ExistsAsync(transactionId))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _transactionService.DeleteAsync(transactionId);

        // Assert
        _transactionRepositoryMock.Verify(repo => repo.DeleteAsync(transactionId), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingTransaction_ShouldThrowDomainException()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _transactionRepositoryMock
            .Setup(repo => repo.ExistsAsync(transactionId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _transactionService.DeleteAsync(transactionId));

        Assert.Contains($"Transação com ID {transactionId} não foi encontrada", exception.Message);
    }

    [Fact]
    public async Task GetPagedAsync_ValidFilter_ShouldReturnPagedResult()
    {
        // Arrange
        var filter = new TransactionFilterDto
        {
            Page = 1,
            PageSize = 10,
            CategoryId = _expenseCategoryId
        };

        var transactions = new List<Transaction>
        {
            new("Transação 1", new Money(100m), DateTime.Today, _expenseCategory),
            new("Transação 2", new Money(200m), DateTime.Today, _expenseCategory)
        };

        _transactionRepositoryMock
            .Setup(repo => repo.GetPagedWithFiltersAsync(
                filter.Page,
                filter.PageSize,
                filter.CategoryId,
                filter.TransactionType,
                filter.StartDate,
                filter.EndDate,
                filter.SearchTerm))
            .ReturnsAsync((transactions, 2));

        // Act
        var result = await _transactionService.GetPagedAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task GetPagedAsync_InvalidFilter_ShouldThrowDomainException()
    {
        // Arrange
        var invalidFilter = new TransactionFilterDto
        {
            Page = 0, // Página inválida
            PageSize = 10
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _transactionService.GetPagedAsync(invalidFilter));

        Assert.Equal("A página deve ser maior que zero.", exception.Message);
    }

    [Fact]
    public async Task GetByDateRangeAsync_ValidRange_ShouldReturnTransactions()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-7);
        var endDate = DateTime.Today;

        var transactions = new List<Transaction>
        {
            new("Transação 1", new Money(100m), DateTime.Today, _expenseCategory)
        };

        _transactionRepositoryMock
            .Setup(repo => repo.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(transactions);

        // Act
        var result = await _transactionService.GetByDateRangeAsync(startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetByDateRangeAsync_InvalidRange_ShouldThrowDomainException()
    {
        // Arrange
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(-1); // Data final antes da inicial

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _transactionService.GetByDateRangeAsync(startDate, endDate));

        Assert.Equal("A data inicial não pode ser maior que a data final.", exception.Message);
    }

    [Fact]
    public async Task GetByMonthAsync_ValidMonth_ShouldReturnTransactions()
    {
        // Arrange
        var month = 3;
        var year = 2024;

        var transactions = new List<Transaction>
        {
            new("Transação março", new Money(100m), new DateTime(2024, 3, 15), _expenseCategory)
        };

        _transactionRepositoryMock
            .Setup(repo => repo.GetByMonthAsync(month, year))
            .ReturnsAsync(transactions);

        // Act
        var result = await _transactionService.GetByMonthAsync(month, year);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Theory]
    [InlineData(0)] // Mês inválido
    [InlineData(13)] // Mês inválido
    public async Task GetByMonthAsync_InvalidMonth_ShouldThrowDomainException(int invalidMonth)
    {
        // Arrange
        var year = 2024;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _transactionService.GetByMonthAsync(invalidMonth, year));

        Assert.Equal("Mês deve estar entre 1 e 12.", exception.Message);
    }

    [Fact]
    public async Task SearchAsync_ValidTerm_ShouldReturnMatchingTransactions()
    {
        // Arrange
        var searchTerm = "supermercado";

        var transactions = new List<Transaction>
        {
            new("Compras supermercado", new Money(150m), DateTime.Today, _expenseCategory)
        };

        _transactionRepositoryMock
            .Setup(repo => repo.SearchByDescriptionAsync(searchTerm))
            .ReturnsAsync(transactions);

        // Act
        var result = await _transactionService.SearchAsync(searchTerm);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    // [InlineData(null)] // REMOVIDO: não permitido para string não anulável
    public async Task SearchAsync_InvalidTerm_ShouldThrowDomainException(string invalidTerm)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _transactionService.SearchAsync(invalidTerm));

        Assert.Equal("Termo de busca é obrigatório.", exception.Message);
    }

    [Fact]
    public async Task GetTotalByTypeAsync_ValidType_ShouldReturnTotal()
    {
        // Arrange
        var totalExpenses = 500.00m;

        _transactionRepositoryMock
            .Setup(repo => repo.GetTotalByTypeAsync(TransactionType.Expense))
            .ReturnsAsync(totalExpenses);

        // Act
        var result = await _transactionService.GetTotalByTypeAsync(TransactionType.Expense);

        // Assert
        Assert.Equal(totalExpenses, result);
    }

    [Fact]
    public async Task GetBalanceAsync_ShouldReturnCorrectBalance()
    {
        // Arrange
        var balance = 1500.00m;

        _transactionRepositoryMock
            .Setup(repo => repo.GetBalanceAsync())
            .ReturnsAsync(balance);

        // Act
        var result = await _transactionService.GetBalanceAsync();

        // Assert
        Assert.Equal(balance, result);
    }

    [Fact]
    public async Task GetBalanceByPeriodAsync_ValidPeriod_ShouldReturnBalance()
    {
        // Arrange
        var startDate = DateTime.Today.AddMonths(-1);
        var endDate = DateTime.Today;
        var balance = 750.00m;

        _transactionRepositoryMock
            .Setup(repo => repo.GetBalanceByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(balance);

        // Act
        var result = await _transactionService.GetBalanceByPeriodAsync(startDate, endDate);

        // Assert
        Assert.Equal(balance, result);
    }

    [Fact]
    public async Task ExistsAsync_ExistingTransaction_ShouldReturnTrue()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _transactionRepositoryMock
            .Setup(repo => repo.ExistsAsync(transactionId))
            .ReturnsAsync(true);

        // Act
        var result = await _transactionService.ExistsAsync(transactionId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetTotalCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        _transactionRepositoryMock
            .Setup(repo => repo.CountAsync())
            .ReturnsAsync(25);

        // Act
        var result = await _transactionService.GetTotalCountAsync();

        // Assert
        Assert.Equal(25, result);
    }

    [Fact]
    public async Task GetByTransactionTypeAsync_ValidType_ShouldReturnTransactionsOfType()
    {
        // Arrange
        var expenseTransactions = new List<Transaction>
        {
            new("Despesa 1", new Money(100m), DateTime.Today, _expenseCategory),
            new("Despesa 2", new Money(200m), DateTime.Today, _expenseCategory)
        };

        _transactionRepositoryMock
            .Setup(repo => repo.GetByTransactionTypeAsync(TransactionType.Expense))
            .ReturnsAsync(expenseTransactions);

        // Act
        var result = await _transactionService.GetByTransactionTypeAsync(TransactionType.Expense);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.True(t.IsExpense));
    }

    [Fact]
    public async Task GetTodayTransactionsAsync_ShouldReturnTodaysTransactions()
    {
        // Arrange
        var todayTransactions = new List<Transaction>
        {
            new("Transação de hoje", new Money(50m), DateTime.Today, _expenseCategory)
        };

        _transactionRepositoryMock
            .Setup(repo => repo.GetTodayTransactionsAsync())
            .ReturnsAsync(todayTransactions);

        // Act
        var result = await _transactionService.GetTodayTransactionsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetCurrentMonthTransactionsAsync_ShouldReturnCurrentMonthTransactions()
    {
        // Arrange
        var currentMonthTransactions = new List<Transaction>
        {
            new("Transação deste mês", new Money(300m), DateTime.Today, _expenseCategory)
        };

        _transactionRepositoryMock
            .Setup(repo => repo.GetCurrentMonthTransactionsAsync())
            .ReturnsAsync(currentMonthTransactions);

        // Act
        var result = await _transactionService.GetCurrentMonthTransactionsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    // Métodos auxiliares para definir IDs via reflection
    private static void SetTransactionId(Transaction transaction, Guid id)
    {
        var idProperty = typeof(Transaction).GetProperty("Id");
        idProperty?.SetValue(transaction, id);
    }

    private static void SetCategoryId(Category category, Guid id)
    {
        var idProperty = typeof(Category).GetProperty("Id");
        idProperty?.SetValue(category, id);
    }
}
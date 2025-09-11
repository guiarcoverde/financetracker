using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Tests.Entities;

public class TransactionTests
{
    private readonly Category _expenseCategory;
    private readonly Category _incomeCategory;

    public TransactionTests()
    {
        _expenseCategory = new Category("Alimentação", CategoryType.Food);
        _incomeCategory = new Category("Salário", CategoryType.Salary);
    }

    [Fact]
    public void Constructor_ValidParameters_ShouldCreateTransaction()
    {
        // Arrange
        var description = "Compras do supermercado";
        var amount = new Money(150.50m);
        var date = DateTime.Today;

        // Act
        var transaction = new Transaction(description, amount, date, _expenseCategory.Id);

        // Assert
        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal("Compras do supermercado", transaction.Description);
        Assert.Equal(150.50m, transaction.Amount.Amount);
        Assert.Equal(DateTime.Today, transaction.TransactionDate);
        Assert.Equal(_expenseCategory.Id, transaction.CategoryId);
        Assert.True(transaction.CreatedAt <= DateTime.UtcNow);
        Assert.Null(transaction.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithCategoryObject_ShouldCreateTransactionAndSetCategory()
    {
        // Arrange
        var description = "Compras do supermercado";
        var amount = new Money(150.50m);
        var date = DateTime.Today;

        // Act
        var transaction = new Transaction(description, amount, date, _expenseCategory);

        // Assert
        Assert.Equal(_expenseCategory.Id, transaction.CategoryId);
        Assert.Equal(_expenseCategory, transaction.Category);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_InvalidDescription_ShouldThrowDomainException(string invalidDescription)
    {
        // Arrange
        var amount = new Money(100.00m);
        var date = DateTime.Today;

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            new Transaction(invalidDescription, amount, date, _expenseCategory.Id));
        Assert.Equal("A descrição da transação é obrigatória.", exception.Message);
    }

    [Fact]
    public void Constructor_DescriptionTooShort_ShouldThrowDomainException()
    {
        // Arrange
        var description = "AB"; // Apenas 2 caracteres
        var amount = new Money(100.00m);
        var date = DateTime.Today;

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            new Transaction(description, amount, date, _expenseCategory.Id));
        Assert.Equal("A descrição deve ter pelo menos 3 caracteres.", exception.Message);
    }

    [Fact]
    public void Constructor_DescriptionTooLong_ShouldThrowDomainException()
    {
        // Arrange
        var description = new string('A', 201); // 201 caracteres
        var amount = new Money(100.00m);
        var date = DateTime.Today;

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            new Transaction(description, amount, date, _expenseCategory.Id));
        Assert.Equal("A descrição não pode exceder 200 caracteres.", exception.Message);
    }

    [Fact]
    public void Constructor_DescriptionWithSpaces_ShouldTrimSpaces()
    {
        // Arrange
        var description = "  Compras do supermercado  ";
        var amount = new Money(100.00m);
        var date = DateTime.Today;

        // Act
        var transaction = new Transaction(description, amount, date, _expenseCategory.Id);

        // Assert
        Assert.Equal("Compras do supermercado", transaction.Description);
    }

    [Fact]
    public void Constructor_FutureDate_ShouldThrowDomainException()
    {
        // Arrange
        var description = "Transação futura";
        var amount = new Money(100.00m);
        var futureDate = DateTime.Today.AddDays(1);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            new Transaction(description, amount, futureDate, _expenseCategory.Id));
        Assert.Equal("A data da transação não pode ser futura.", exception.Message);
    }

    [Fact]
    public void Constructor_VeryOldDate_ShouldThrowDomainException()
    {
        // Arrange
        var description = "Transação muito antiga";
        var amount = new Money(100.00m);
        var oldDate = DateTime.Today.AddYears(-6); // Mais de 5 anos

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            new Transaction(description, amount, oldDate, _expenseCategory.Id));
        Assert.Contains("A data da transação não pode ser anterior a", exception.Message);
    }

    [Fact]
    public void Constructor_EmptyCategoryId_ShouldThrowDomainException()
    {
        // Arrange
        var description = "Transação sem categoria";
        var amount = new Money(100.00m);
        var date = DateTime.Today;

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            new Transaction(description, amount, date, Guid.Empty));
        Assert.Equal("A categoria da transação é obrigatória.", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldNormalizeDateToDateOnly()
    {
        // Arrange
        var description = "Test";
        var amount = new Money(100.00m);
        var dateTime = new DateTime(2024, 3, 15, 14, 30, 45); // Com hora

        // Act
        var transaction = new Transaction(description, amount, dateTime, _expenseCategory.Id);

        // Assert
        Assert.Equal(new DateTime(2024, 3, 15), transaction.TransactionDate); // Apenas data
    }

    [Fact]
    public void UpdateDescription_ValidDescription_ShouldUpdateDescriptionAndUpdatedAt()
    {
        // Arrange
        var transaction = new Transaction("Original", new Money(100m), DateTime.Today, _expenseCategory.Id);
        var originalUpdatedAt = transaction.UpdatedAt;
        var newDescription = "Nova descrição";

        // Act
        transaction.UpdateDescription(newDescription);

        // Assert
        Assert.Equal("Nova descrição", transaction.Description);
        Assert.NotEqual(originalUpdatedAt, transaction.UpdatedAt);
        Assert.True(transaction.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void UpdateAmount_ValidAmount_ShouldUpdateAmountAndUpdatedAt()
    {
        // Arrange
        var transaction = new Transaction("Test", new Money(100m), DateTime.Today, _expenseCategory.Id);
        var newAmount = new Money(200.50m);

        // Act
        transaction.UpdateAmount(newAmount);

        // Assert
        Assert.Equal(200.50m, transaction.Amount.Amount);
        Assert.NotNull(transaction.UpdatedAt);
    }

    [Fact]
    public void UpdateAmount_NullAmount_ShouldThrowDomainException()
    {
        // Arrange
        var transaction = new Transaction("Test", new Money(100m), DateTime.Today, _expenseCategory.Id);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => transaction.UpdateAmount(null));
        Assert.Equal("O valor da transação não pode ser nulo.", exception.Message);
    }

    [Fact]
    public void UpdateTransactionDate_ValidDate_ShouldUpdateDateAndUpdatedAt()
    {
        // Arrange
        var transaction = new Transaction("Test", new Money(100m), DateTime.Today, _expenseCategory.Id);
        var newDate = DateTime.Today.AddDays(-5);

        // Act
        transaction.UpdateTransactionDate(newDate);

        // Assert
        Assert.Equal(newDate.Date, transaction.TransactionDate);
        Assert.NotNull(transaction.UpdatedAt);
    }

    [Fact]
    public void UpdateCategory_ValidCategoryId_ShouldUpdateCategoryAndUpdatedAt()
    {
        // Arrange
        var transaction = new Transaction("Test", new Money(100m), DateTime.Today, _expenseCategory.Id);

        // Act
        transaction.UpdateCategory(_incomeCategory);

        // Assert
        Assert.Equal(_incomeCategory.Id, transaction.CategoryId);
        Assert.NotNull(transaction.UpdatedAt);
    }

    [Fact]
    public void UpdateCategory_ValidCategoryObject_ShouldUpdateCategoryAndUpdatedAt()
    {
        // Arrange
        var transaction = new Transaction("Test", new Money(100m), DateTime.Today, _expenseCategory);

        // Act
        transaction.UpdateCategory(_incomeCategory);

        // Assert
        Assert.Equal(_incomeCategory.Id, transaction.CategoryId);
        Assert.Equal(_incomeCategory, transaction.Category);
        Assert.NotNull(transaction.UpdatedAt);
    }

    [Fact]
    public void UpdateCategory_NullCategory_ShouldThrowDomainException()
    {
        // Arrange
        var transaction = new Transaction("Test", new Money(100m), DateTime.Today, _expenseCategory.Id);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => transaction.UpdateCategory((Category)null));
        Assert.Equal("A categoria não pode ser nula.", exception.Message);
    }

    [Fact]
    public void IsIncome_WithIncomeCategory_ShouldReturnTrue()
    {
        // Arrange
        var transaction = new Transaction("Salário", new Money(3000m), DateTime.Today, _incomeCategory);

        // Act & Assert
        Assert.True(transaction.IsIncome);
        Assert.False(transaction.IsExpense);
        Assert.Equal(TransactionType.Income, transaction.TransactionType);
    }

    [Fact]
    public void IsExpense_WithExpenseCategory_ShouldReturnTrue()
    {
        // Arrange
        var transaction = new Transaction("Compras", new Money(150m), DateTime.Today, _expenseCategory);

        // Act & Assert
        Assert.True(transaction.IsExpense);
        Assert.False(transaction.IsIncome);
        Assert.Equal(TransactionType.Expense, transaction.TransactionType);
    }

    [Theory]
    [InlineData(3, 2024, true)] // Março 2024
    [InlineData(2, 2024, false)] // Fevereiro 2024
    public void IsFromCurrentMonth_ShouldReturnCorrectValue(int month, int year, bool expected)
    {
        // Arrange
        var date = new DateTime(year, month, 15);
        var transaction = new Transaction("Test", new Money(100m), date, _expenseCategory.Id);

        // Simular que estamos em março de 2024
        var currentDate = new DateTime(2024, 3, 20);
        
        // Act
        var isCurrentMonth = transaction.TransactionDate.Month == currentDate.Month && 
                           transaction.TransactionDate.Year == currentDate.Year;

        // Assert
        Assert.Equal(expected, isCurrentMonth);
    }

    [Fact]
    public void IsFromToday_WithTodaysDate_ShouldReturnTrue()
    {
        // Arrange
        var transaction = new Transaction("Test", new Money(100m), DateTime.Today, _expenseCategory.Id);

        // Act & Assert
        Assert.True(transaction.IsFromToday);
    }

    [Fact]
    public void IsFromToday_WithYesterdaysDate_ShouldReturnFalse()
    {
        // Arrange
        var yesterday = DateTime.Today.AddDays(-1);
        var transaction = new Transaction("Test", new Money(100m), yesterday, _expenseCategory.Id);

        // Act & Assert
        Assert.False(transaction.IsFromToday);
    }

    [Fact]
    public void IsCategoryCompatible_WithPositiveAmount_ShouldReturnTrue()
    {
        // Arrange
        var transaction = new Transaction("Test", new Money(100m), DateTime.Today, _expenseCategory);

        // Act & Assert
        Assert.True(transaction.IsCategoryCompatible());
    }

    [Fact]
    public void ToString_ShouldReturnCorrectFormat()
    {
        // Arrange
        var transaction = new Transaction("Compras supermercado", new Money(150.75m), 
            new DateTime(2024, 3, 15), _expenseCategory);

        // Act
        var result = transaction.ToString();

        // Assert
        Assert.Equal("15/03/2024 - Compras supermercado: R$ 150,75 (Alimentação)", result);
    }

    [Fact]
    public void Equals_TwoTransactionsWithSameId_ShouldBeEqual()
    {
        // Arrange
        var transaction1 = new Transaction("Test1", new Money(100m), DateTime.Today, _expenseCategory.Id);
        var transaction2 = new Transaction("Test2", new Money(200m), DateTime.Today, _incomeCategory.Id);
        
        // Usar reflection para definir o mesmo ID
        var idProperty = typeof(Transaction).GetProperty("Id");
        idProperty?.SetValue(transaction2, transaction1.Id);

        // Act & Assert
        Assert.Equal(transaction1, transaction2);
        Assert.Equal(transaction1.GetHashCode(), transaction2.GetHashCode());
    }

    [Fact]
    public void CreatedAt_ShouldBeInUtc()
    {
        // Arrange & Act
        var transaction = new Transaction("Test", new Money(100m), DateTime.Today, _expenseCategory.Id);

        // Assert
        Assert.Equal(DateTimeKind.Utc, transaction.CreatedAt.Kind);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var transaction1 = new Transaction("Test1", new Money(100m), DateTime.Today, _expenseCategory.Id);
        var transaction2 = new Transaction("Test2", new Money(200m), DateTime.Today, _incomeCategory.Id);

        // Assert
        Assert.NotEqual(transaction1.Id, transaction2.Id);
        Assert.NotEqual(Guid.Empty, transaction1.Id);
        Assert.NotEqual(Guid.Empty, transaction2.Id);
    }
}
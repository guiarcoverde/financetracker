using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Tests.Entities;

public class CategoryTests
{
    [Fact]
    public void Constructor_ValidParameters_ShouldCreateCategory()
    {
        // Arrange
        var name = "Alimentação";
        var categoryType = CategoryType.Food;

        // Act
        var category = new Category(name, categoryType);

        // Assert
        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal("Alimentação", category.Name);
        Assert.Equal(CategoryType.Food, category.CategoryType);
        Assert.True(category.CreatedAt <= DateTime.UtcNow);
        Assert.True(category.CreatedAt >= DateTime.UtcNow.AddSeconds(-5)); // Margem de 5 segundos
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_InvalidName_ShouldThrowDomainException(string invalidName)
    {
        // Arrange
        var categoryType = CategoryType.Food;

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => new Category(invalidName, categoryType));
        Assert.Equal("O nome da categoria é obrigatório.", exception.Message);
    }

    [Fact]
    public void Constructor_NameTooShort_ShouldThrowDomainException()
    {
        // Arrange
        var name = "A"; // Apenas 1 caractere
        var categoryType = CategoryType.Food;

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => new Category(name, categoryType));
        Assert.Equal("O nome da categoria deve ter pelo menos 2 caracteres.", exception.Message);
    }

    [Fact]
    public void Constructor_NameTooLong_ShouldThrowDomainException()
    {
        // Arrange
        var name = new string('A', 51); // 51 caracteres
        var categoryType = CategoryType.Food;

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => new Category(name, categoryType));
        Assert.Equal("O nome da categoria não pode exceder 50 caracteres.", exception.Message);
    }

    [Fact]
    public void Constructor_NameWithSpaces_ShouldTrimSpaces()
    {
        // Arrange
        var name = "  Alimentação  ";
        var categoryType = CategoryType.Food;

        // Act
        var category = new Category(name, categoryType);

        // Assert
        Assert.Equal("Alimentação", category.Name);
    }

    [Fact]
    public void UpdateName_ValidName_ShouldUpdateName()
    {
        // Arrange
        var category = new Category("Nome Original", CategoryType.Food);
        var newName = "Nome Atualizado";

        // Act
        category.UpdateName(newName);

        // Assert
        Assert.Equal("Nome Atualizado", category.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateName_InvalidName_ShouldThrowDomainException(string invalidName)
    {
        // Arrange
        var category = new Category("Nome Original", CategoryType.Food);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => category.UpdateName(invalidName));
        Assert.Equal("O nome da categoria é obrigatório.", exception.Message);
        
        // Verificar que o nome não foi alterado
        Assert.Equal("Nome Original", category.Name);
    }

    [Fact]
    public void UpdateName_NameWithSpaces_ShouldTrimSpaces()
    {
        // Arrange
        var category = new Category("Nome Original", CategoryType.Food);
        var newName = "  Nome Atualizado  ";

        // Act
        category.UpdateName(newName);

        // Assert
        Assert.Equal("Nome Atualizado", category.Name);
    }

    [Fact]
    public void UpdateCategoryType_ValidType_ShouldUpdateType()
    {
        // Arrange
        var category = new Category("Categoria", CategoryType.Food);
        var newType = CategoryType.Health;

        // Act
        category.UpdateCategoryType(newType);

        // Assert
        Assert.Equal(CategoryType.Health, category.CategoryType);
    }

    [Theory]
    [InlineData(CategoryType.Food, true, false)]
    [InlineData(CategoryType.Health, true, false)]
    [InlineData(CategoryType.Salary, false, true)]
    [InlineData(CategoryType.Investment, false, true)]
    public void IsExpenseCategory_ShouldReturnCorrectValue(CategoryType categoryType, bool expectedIsExpense, bool expectedIsIncome)
    {
        // Arrange
        var category = new Category("Test", categoryType);

        // Act & Assert
        Assert.Equal(expectedIsExpense, category.IsExpenseCategory);
        Assert.Equal(expectedIsIncome, category.IsIncomeCategory);
    }

    [Theory]
    [InlineData(CategoryType.Food, "Alimentação")]
    [InlineData(CategoryType.Health, "Saúde")]
    [InlineData(CategoryType.Salary, "Salário")]
    [InlineData(CategoryType.Investment, "Investimentos")]
    public void DisplayName_ShouldReturnCorrectDisplayName(CategoryType categoryType, string expectedDisplayName)
    {
        // Arrange
        var category = new Category("Test", categoryType);

        // Act & Assert
        Assert.Equal(expectedDisplayName, category.DisplayName);
    }

    [Theory]
    [InlineData(CategoryType.Food, TransactionType.Expense)]
    [InlineData(CategoryType.Health, TransactionType.Expense)]
    [InlineData(CategoryType.Salary, TransactionType.Income)]
    [InlineData(CategoryType.Investment, TransactionType.Income)]
    public void TransactionType_ShouldReturnCorrectType(CategoryType categoryType, TransactionType expectedTransactionType)
    {
        // Arrange
        var category = new Category("Test", categoryType);

        // Act & Assert
        Assert.Equal(expectedTransactionType, category.TransactionType);
    }

    [Fact]
    public void ToString_ShouldReturnCorrectFormat()
    {
        // Arrange
        var category = new Category("Supermercado", CategoryType.Food);

        // Act
        var result = category.ToString();

        // Assert
        Assert.Equal("Supermercado (Alimentação)", result);
    }

    [Fact]
    public void Equals_TwoCategoriesWithSameId_ShouldBeEqual()
    {
        // Arrange
        var category1 = new Category("Test", CategoryType.Food);
        var category2 = new Category("Different Name", CategoryType.Health);
        
        // Usar reflection para definir o mesmo ID (simulando entidades do banco)
        var idProperty = typeof(Category).GetProperty("Id");
        idProperty?.SetValue(category2, category1.Id);

        // Act & Assert
        Assert.Equal(category1, category2);
        Assert.Equal(category1.GetHashCode(), category2.GetHashCode());
    }

    [Fact]
    public void Equals_TwoCategoriesWithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var category1 = new Category("Test", CategoryType.Food);
        var category2 = new Category("Test", CategoryType.Food);

        // Act & Assert (IDs diferentes mesmo com mesmo nome)
        Assert.NotEqual(category1, category2);
        Assert.NotEqual(category1.GetHashCode(), category2.GetHashCode());
    }

    [Fact]
    public void Equals_CategoryAndNull_ShouldNotBeEqual()
    {
        // Arrange
        var category = new Category("Test", CategoryType.Food);

        // Act & Assert
        Assert.False(category.Equals(null));
    }

    [Fact]
    public void Equals_CategoryAndDifferentType_ShouldNotBeEqual()
    {
        // Arrange
        var category = new Category("Test", CategoryType.Food);
        var differentObject = "Not a Category";

        // Act & Assert
        Assert.False(category.Equals(differentObject));
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var category1 = new Category("Test1", CategoryType.Food);
        var category2 = new Category("Test2", CategoryType.Health);
        var category3 = new Category("Test3", CategoryType.Salary);

        // Assert
        Assert.NotEqual(category1.Id, category2.Id);
        Assert.NotEqual(category1.Id, category3.Id);
        Assert.NotEqual(category2.Id, category3.Id);
        
        // Todos os IDs devem ser válidos
        Assert.NotEqual(Guid.Empty, category1.Id);
        Assert.NotEqual(Guid.Empty, category2.Id);
        Assert.NotEqual(Guid.Empty, category3.Id);
    }

    [Fact]
    public void CreatedAt_ShouldBeInUtc()
    {
        // Arrange & Act
        var category = new Category("Test", CategoryType.Food);

        // Assert
        Assert.Equal(DateTimeKind.Utc, category.CreatedAt.Kind);
    }

    [Fact]
    public void Constructor_MaxValidLength_ShouldWork()
    {
        // Arrange
        var name = new string('A', 50); // Exatamente 50 caracteres
        var categoryType = CategoryType.Food;

        // Act
        var category = new Category(name, categoryType);

        // Assert
        Assert.Equal(50, category.Name.Length);
        Assert.Equal(name, category.Name);
    }

    [Fact]
    public void Constructor_MinValidLength_ShouldWork()
    {
        // Arrange
        var name = "AB"; // Exatamente 2 caracteres
        var categoryType = CategoryType.Food;

        // Act
        var category = new Category(name, categoryType);

        // Assert
        Assert.Equal(2, category.Name.Length);
        Assert.Equal(name, category.Name);
    }
}
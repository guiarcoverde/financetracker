using FinanceTracker.Application.DTOs.Category;
using FinanceTracker.Application.Services.Implementations;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Domain.ValueObjects;
using Mapster;
using Moq;

namespace FinanceTracker.Application.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        
        // Configurar o UnitOfWork para retornar o repositório mock
        _unitOfWorkMock.Setup(uow => uow.Categories).Returns(_categoryRepositoryMock.Object);
        
        _categoryService = new CategoryService(_unitOfWorkMock.Object);

        // Configurar Mapster para os testes
        ConfigureMapster();
    }

    private static void ConfigureMapster()
    {
        TypeAdapterConfig<Category, CategoryDto>
            .NewConfig()
            .Map(dest => dest.DisplayName, src => src.DisplayName)
            .Map(dest => dest.IsExpenseCategory, src => src.IsExpenseCategory)
            .Map(dest => dest.IsIncomeCategory, src => src.IsIncomeCategory)
            .Map(dest => dest.TransactionType, src => src.TransactionType);

        TypeAdapterConfig<Category, CategorySummaryDto>
            .NewConfig()
            .Map(dest => dest.DisplayName, src => src.DisplayName)
            .Map(dest => dest.TransactionType, src => src.TransactionType);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCategory_ShouldReturnCategoryDto()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category("Alimentação", CategoryType.Food);
        SetCategoryId(category, categoryId);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        // Act
        var result = await _categoryService.GetByIdAsync(categoryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categoryId, result.Id);
        Assert.Equal("Alimentação", result.Name);
        Assert.Equal(CategoryType.Food, result.CategoryType);
        Assert.Equal("Alimentação", result.DisplayName);
        Assert.True(result.IsExpenseCategory);
        Assert.False(result.IsIncomeCategory);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingCategory_ShouldThrowDomainException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => 
            _categoryService.GetByIdAsync(categoryId));
        
        Assert.Contains($"Categoria com ID {categoryId} não foi encontrada", exception.Message);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new("Alimentação", CategoryType.Food),
            new("Saúde", CategoryType.Health),
            new("Salário", CategoryType.Salary)
        };

        _categoryRepositoryMock
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _categoryService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        
        var resultList = result.ToList();
        Assert.Equal("Alimentação", resultList[0].Name);
        Assert.Equal("Saúde", resultList[1].Name);
        Assert.Equal("Salário", resultList[2].Name);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ShouldCreateAndReturnCategory()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "Nova Categoria",
            CategoryType = CategoryType.Transport
        };

        var createdCategory = new Category(createDto.Name, createDto.CategoryType);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByNameAsync(createDto.Name.Trim()))
            .ReturnsAsync((Category?)null); // Nome não existe

        _categoryRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Category>()))
            .ReturnsAsync(createdCategory);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _categoryService.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Nova Categoria", result.Name);
        Assert.Equal(CategoryType.Transport, result.CategoryType);

        // Verificar se os métodos foram chamados
        _categoryRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Category>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NullDto_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _categoryService.CreateAsync(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateAsync_InvalidName_ShouldThrowDomainException(string invalidName)
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = invalidName,
            CategoryType = CategoryType.Food
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => 
            _categoryService.CreateAsync(createDto));
        
        Assert.Equal("O nome da categoria é obrigatório.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ShouldThrowDomainException()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "Categoria Existente",
            CategoryType = CategoryType.Food
        };

        var existingCategory = new Category("Categoria Existente", CategoryType.Health);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByNameAsync("Categoria Existente"))
            .ReturnsAsync(existingCategory);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => 
            _categoryService.CreateAsync(createDto));
        
        Assert.Contains("Já existe uma categoria com o nome", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ValidDto_ShouldUpdateAndReturnCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDto
        {
            Name = "Nome Atualizado",
            CategoryType = CategoryType.Health
        };

        var existingCategory = new Category("Nome Original", CategoryType.Food);
        SetCategoryId(existingCategory, categoryId);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(existingCategory);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByNameAsync("Nome Atualizado"))
            .ReturnsAsync((Category?)null); // Nome não existe em outra categoria

        _categoryRepositoryMock
            .Setup(repo => repo.UpdateAsync(existingCategory))
            .ReturnsAsync(existingCategory);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _categoryService.UpdateAsync(categoryId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Nome Atualizado", result.Name);
        Assert.Equal(CategoryType.Health, result.CategoryType);

        // Verificar se os métodos foram chamados
        _categoryRepositoryMock.Verify(repo => repo.UpdateAsync(existingCategory), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingCategory_ShouldThrowDomainException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDto
        {
            Name = "Nome Atualizado",
            CategoryType = CategoryType.Health
        };

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => 
            _categoryService.UpdateAsync(categoryId, updateDto));
        
        Assert.Contains($"Categoria com ID {categoryId} não foi encontrada", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ExistingCategoryWithoutTransactions_ShouldDelete()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _categoryRepositoryMock
            .Setup(repo => repo.ExistsAsync(categoryId))
            .ReturnsAsync(true);

        _categoryRepositoryMock
            .Setup(repo => repo.HasTransactionAsync(categoryId))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _categoryService.DeleteAsync(categoryId);

        // Assert
        _categoryRepositoryMock.Verify(repo => repo.DeleteAsync(categoryId), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CategoryWithTransactions_ShouldThrowDomainException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _categoryRepositoryMock
            .Setup(repo => repo.ExistsAsync(categoryId))
            .ReturnsAsync(true);

        _categoryRepositoryMock
            .Setup(repo => repo.HasTransactionAsync(categoryId))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => 
            _categoryService.DeleteAsync(categoryId));
        
        Assert.Equal("Não é possível excluir uma categoria que possui transações associadas.", exception.Message);
    }

    [Fact]
    public async Task GetByTransactionTypeAsync_ExpenseType_ShouldReturnExpenseCategories()
    {
        // Arrange
        var expenseCategories = new List<Category>
        {
            new("Alimentação", CategoryType.Food),
            new("Transporte", CategoryType.Transport)
        };

        _categoryRepositoryMock
            .Setup(repo => repo.GetByTypeAsync(TransactionType.Expense))
            .ReturnsAsync(expenseCategories);

        // Act
        var result = await _categoryService.GetByTransactionTypeAsync(TransactionType.Expense);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, category => Assert.True(category.IsExpenseCategory));
    }

    [Fact]
    public async Task GetSummariesAsync_ShouldReturnCategorySummaries()
    {
        // Arrange
        var categories = new List<Category>
        {
            new("Alimentação", CategoryType.Food),
            new("Salário", CategoryType.Salary)
        };

        _categoryRepositoryMock
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _categoryService.GetSummariesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());

        var summaries = result.ToList();
        Assert.Equal("Alimentação", summaries[0].Name);
        Assert.Equal("Salário", summaries[1].Name);
    }

    [Fact]
    public async Task ExistsAsync_ExistingCategory_ShouldReturnTrue()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _categoryRepositoryMock
            .Setup(repo => repo.ExistsAsync(categoryId))
            .ReturnsAsync(true);

        // Act
        var result = await _categoryService.ExistsAsync(categoryId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanDeleteAsync_CategoryWithoutTransactions_ShouldReturnTrue()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _categoryRepositoryMock
            .Setup(repo => repo.ExistsAsync(categoryId))
            .ReturnsAsync(true);

        _categoryRepositoryMock
            .Setup(repo => repo.HasTransactionAsync(categoryId))
            .ReturnsAsync(false);

        // Act
        var result = await _categoryService.CanDeleteAsync(categoryId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanDeleteAsync_NonExistingCategory_ShouldReturnFalse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _categoryRepositoryMock
            .Setup(repo => repo.ExistsAsync(categoryId))
            .ReturnsAsync(false);

        // Act
        var result = await _categoryService.CanDeleteAsync(categoryId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetExpenseCategoriesAsync_ShouldReturnExpenseCategories()
    {
        // Arrange
        var expenseCategories = new List<Category>
        {
            new("Alimentação", CategoryType.Food)
        };

        _categoryRepositoryMock
            .Setup(repo => repo.GetByTypeAsync(TransactionType.Expense))
            .ReturnsAsync(expenseCategories);

        // Act
        var result = await _categoryService.GetExpenseCategoriesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result.First().IsExpenseCategory);
    }

    [Fact]
    public async Task GetIncomeCategoriesAsync_ShouldReturnIncomeCategories()
    {
        // Arrange
        var incomeCategories = new List<Category>
        {
            new("Salário", CategoryType.Salary)
        };

        _categoryRepositoryMock
            .Setup(repo => repo.GetByTypeAsync(TransactionType.Income))
            .ReturnsAsync(incomeCategories);

        // Act
        var result = await _categoryService.GetIncomeCategoriesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result.First().IsIncomeCategory);
    }

    [Fact]
    public async Task GetTotalCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        _categoryRepositoryMock
            .Setup(repo => repo.CountAsync())
            .ReturnsAsync(5);

        // Act
        var result = await _categoryService.GetTotalCountAsync();

        // Assert
        Assert.Equal(5, result);
    }

    // Método auxiliar para definir ID da categoria via reflection
    private static void SetCategoryId(Category category, Guid id)
    {
        var idProperty = typeof(Category).GetProperty("Id");
        idProperty?.SetValue(category, id);
    }
}
using FinanceTracker.Application.DTOs.Category;
using FinanceTracker.Application.Services.Interfaces;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Domain.ValueObjects;
using Mapster;

namespace FinanceTracker.Application.Services.Implementations;

public class CategoryService(IUnitOfWork unitOfWork) : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;


    public async Task<CategoryDto> GetByIdAsync(Guid id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            throw new DomainException($"Categoria com ID {id} não foi encontrada.");
        }
        
        return category.Adapt<CategoryDto>();
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return categories.Select(c => c.Adapt<CategoryDto>()).ToList();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto createDto)
    {
        await ValidateCreateDto(createDto);
        
        var category = new Category(createDto.Name, createDto.CategoryType);
        var createdCategory = await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();
        
        return createdCategory.Adapt<CategoryDto>();
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto updateDto)
    {
        await ValidateUpdateDto(id, updateDto);
        
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            throw new DomainException($"Categoria com ID {id} não foi encontrada.");
        }
        
        category.UpdateName(updateDto.Name);
        category.UpdateCategoryType(updateDto.CategoryType);
        
        var updatedCategory = await _unitOfWork.Categories.UpdateAsync(category);
        
        return updatedCategory.Adapt<CategoryDto>();
    }

    public async Task DeleteAsync(Guid id)
    {
        await ValidateForDeletionAsync(id);

        await _unitOfWork.Categories.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<CategorySummaryDto>> GetSummariesAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return categories.Select(c => c.Adapt<CategorySummaryDto>()).ToList();
    }

    public async Task<IEnumerable<CategoryDto>> GetByTransactionTypeAsync(TransactionType transactionType)
    {
        var categories = await _unitOfWork.Categories.GetByTypeAsync(transactionType);
        return categories.Select(c => c.Adapt<CategoryDto>()).ToList();
    }

    public async Task<IEnumerable<CategoryDto>> GetExpenseCategoriesAsync()
    {
        return await GetByTransactionTypeAsync(TransactionType.Expense);
    }

    public async Task<IEnumerable<CategoryDto>> GetIncomeCategoriesAsync()
    {
        return await GetByTransactionTypeAsync(TransactionType.Income);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _unitOfWork.Categories.ExistsAsync(id);
    }

    public async Task<bool> CanDeleteAsync(Guid id)
    {
        if (!await ExistsAsync(id))
            return false;
        
        return !await _unitOfWork.Categories.HasTransactionAsync(id);
    }

    public async Task ValidateForDeletionAsync(Guid id)
    {
        if(!await ExistsAsync(id))
            throw new DomainException($"Categoria com ID {id} não foi encontrada.");

        if (await _unitOfWork.Categories.HasTransactionAsync(id))
            throw new DomainException("Não é possível excluir uma categoria que possui transações associadas.");
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesWithTransactionsAsync(Guid categoryId)
    {
        var categories = await _unitOfWork.Categories.GetCategoriesWithTransactionsAsync();
        return categories.Select(c => c.Adapt<CategoryDto>()).ToList();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _unitOfWork.Categories.CountAsync();
    }
    
    private async Task ValidateCreateDto(CreateCategoryDto createDto)
    {
        ArgumentNullException.ThrowIfNull(createDto);

        if (string.IsNullOrWhiteSpace(createDto.Name))
            throw new DomainException("O nome da categoria é obrigatório.");
        
        var existingCategory = await _unitOfWork.Categories.GetByNameAsync(createDto.Name.Trim());
        if (existingCategory != null)
            throw new DomainException($"Já existe uma categoria com o nome '{createDto.Name}'.");
        
        if(!Enum.IsDefined(typeof(CategoryType), createDto.CategoryType))
            throw new DomainException("Tipo de categoria inválido.");
    }
    
    private async Task ValidateUpdateDto(Guid id, UpdateCategoryDto updateDto)
    {
        ArgumentNullException.ThrowIfNull(updateDto);

        if(string.IsNullOrWhiteSpace(updateDto.Name))
            throw new DomainException("O nome da categoria é obrigatório.");
        
        var existingCategory = await _unitOfWork.Categories.GetByNameAsync(updateDto.Name.Trim());
        if (existingCategory != null && existingCategory.Id != id)
            throw new DomainException($"Já existe uma categoria com o nome '{updateDto.Name}'.");
        
        if(!Enum.IsDefined(typeof(CategoryType), updateDto.CategoryType))
            throw new DomainException("Tipo de categoria inválido.");
    }
}
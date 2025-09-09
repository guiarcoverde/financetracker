using FinanceTracker.Application.DTOs.Category;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.Services.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto> GetByIdAsync(Guid id);
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto> CreateAsync(CreateCategoryDto createCategoryDto);
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto updateCategoryDto);
    Task DeleteAsync(Guid id);
    
    Task<IEnumerable<CategorySummaryDto>> GetSummariesAsync();
    Task<IEnumerable<CategoryDto>> GetByTransactionTypeAsync(TransactionType transactionType);
    Task<IEnumerable<CategoryDto>> GetExpenseCategoriesAsync();
    Task<IEnumerable<CategoryDto>> GetIncomeCategoriesAsync();
    
    Task<bool> ExistsAsync(Guid id);
    Task<bool> CanDeleteAsync(Guid id);
    Task ValidateForDeletionAsync(Guid id);
    
    Task<IEnumerable<CategoryDto>> GetCategoriesWithTransactionsAsync(Guid categoryId);
    Task<int> GetTotalCountAsync();
}
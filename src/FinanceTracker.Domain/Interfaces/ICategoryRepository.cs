using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id);
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category> AddAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task DeleteAsync(Guid id);
    
    Task<IEnumerable<Category>> GetByTypeAsync(TransactionType transactionType);
    Task<IEnumerable<Category>> GetByCategoryTypeAsync(CategoryType transactionType);
    Task<Category?> GetByNameAsync(string name);
    
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByNameAsync(string name);
    Task<bool> HasTransactionAsync(Guid categoryId);
    
    Task<IEnumerable<Category>> GetCategoriesWithTransactionsAsync();
    Task<int> CountAsync();
}
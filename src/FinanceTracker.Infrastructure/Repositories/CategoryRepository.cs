using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Domain.ValueObjects;
using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Repositories;

public class CategoryRepository(ApplicationDbContext context) : ICategoryRepository
{
    private readonly ApplicationDbContext _context = context;
    
    public async Task<Category?> GetByIdAsync(Guid id) 
        => await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Category>> GetAllAsync()
        => await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    public async Task<Category> AddAsync(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        
        var entry = await _context.Categories.AddAsync(category);
        return entry.Entity;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        var entry = _context.Categories.Update(category);
        return entry.Entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is not null)
        {
            _context.Categories.Remove(category);
        }
    }

    public async Task<IEnumerable<Category>> GetByTypeAsync(TransactionType transactionType)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => (transactionType == TransactionType.Income &&
                         c.CategoryType >= CategoryType.Salary) ||
                        (transactionType == TransactionType.Expense &&
                        c.CategoryType < CategoryType.Salary))
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetByCategoryTypeAsync(CategoryType categoryType)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.CategoryType == categoryType)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name.Equals(name.Trim(), StringComparison.CurrentCultureIgnoreCase));
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
        
        return await _context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Name.Equals(name.Trim(), StringComparison.CurrentCultureIgnoreCase));
    }

    public async Task<bool> HasTransactionAsync(Guid categoryId)
    {
        return await _context.Transactions
            .AsNoTracking()
            .AnyAsync(t => t.CategoryId == categoryId);
    }

    public async Task<IEnumerable<Category>> GetCategoriesWithTransactionsAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => _context.Transactions.Any(t => t.CategoryId == c.Id))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .CountAsync();
    }
}
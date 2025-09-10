using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Domain.ValueObjects;
using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Repositories;

public class TransactionRepository(ApplicationDbContext context) : ITransactionRepository
{
    private readonly ApplicationDbContext _context = context;
    public async Task<Transaction?> GetByIdAsync(Guid id) 
        => await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Transaction?> GetByIdWithCategoryAsync(Guid id) =>
        await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<Transaction>> GetAllAsync() 
        => await _context.Transactions
            .AsNoTracking()
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Transaction>> GetAllWithCategoriesAsync() 
        => await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<Transaction> AddAsync(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        
        var entry = await _context.Transactions.AddAsync(transaction);
        return entry.Entity;
    }

    public async Task<Transaction> UpdateAsync(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        
        var entry = _context.Transactions.Update(transaction);
        return entry.Entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction is not null)
        {
            _context.Transactions.Remove(transaction);
        }
    }

    public async Task<IEnumerable<Transaction>> GetByCategoryIdAsync(Guid categoryId) 
        => await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.CategoryId == categoryId)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Transaction>> GetByTransactionTypeAsync(TransactionType transactionType) =>
        await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.Category != null &&
                        ((transactionType == TransactionType.Income &&
                          t.Category.CategoryType >= CategoryType.Salary) ||
                         (transactionType == TransactionType.Expense &&
                          t.Category.CategoryType < CategoryType.Salary)))
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
                         

    public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate) 
        => await _context.Transactions
        .AsNoTracking()
        .Include(t => t.Category)
        .Where(t => t.TransactionDate >= startDate.Date && t.TransactionDate <= endDate.Date)
        .OrderByDescending(t => t.TransactionDate)
        .ThenByDescending(t => t.CreatedAt)
        .ToListAsync();

    //TODO: Finalizar implementação dos métodos abaixo
    public async Task<IEnumerable<Transaction>> GetByMonthAsync(int month, int year)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Transaction>> GetByYearAsync(int year)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Transaction>> GetTodayTransactionsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Transaction>> GetCurrentMonthTransactionsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Transaction>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Transaction>> GetByCategoryAndDateRangeAsync(Guid categoryId, DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Transaction>> GetByTypeAndDateRange(TransactionType transactionType, DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Transaction>> SearchByDescriptionAsync(string searchTerm)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<decimal> GetTotalByTypeAsync(TransactionType transactionType)
    {
        throw new NotImplementedException();
    }

    public async Task<decimal> GetTotalByTypeAndDateRangeAsync(TransactionType transactionType, DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }

    public async Task<decimal> GetTotalByCategoryAsync(Guid categoryId)
    {
        throw new NotImplementedException();
    }

    public async Task<decimal> GetBalanceAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<decimal> GetBalanceByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }

    public async Task<(IEnumerable<Transaction> Transactions, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<(IEnumerable<Transaction> Transactions, int TotalCount)> GetPagedWithFiltersAsync(int page, int pageSize, Guid? categoryId = null, TransactionType? transactionType = null,
        DateTime? startDate = null, DateTime? endDate = null, string? searchTerm = null)
    {
        throw new NotImplementedException();
    }

    public async Task<int> CountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<int> CountByTypeAsync(TransactionType transactionType)
    {
        throw new NotImplementedException();
    }

    public async Task<int> CountByCategoryAsync(Guid categoryId)
    {
        throw new NotImplementedException();
    }
}
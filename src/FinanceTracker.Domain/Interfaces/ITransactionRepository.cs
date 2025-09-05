using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<Transaction?> GetByIdWithCategoryAsync(Guid id);
    Task<IEnumerable<Transaction>> GetAllAsync();
    Task<IEnumerable<Transaction>> GetAllWithCategoriesAsync();
    Task<Transaction> AddAsync(Transaction transaction);
    Task<Transaction> UpdateAsync(Transaction transaction);
    Task DeleteAsync(Guid id);
    
    Task<IEnumerable<Transaction>> GetByCategoryIdAsync(Guid categoryId);
    Task<IEnumerable<Transaction>> GetByTransactionTypeAsync(TransactionType transactionType);
    
    Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Transaction>> GetByMonthAsync(int month, int year);
    Task<IEnumerable<Transaction>> GetByYearAsync(int year);
    Task<IEnumerable<Transaction>> GetTodayTransactionsAsync();
    Task<IEnumerable<Transaction>> GetCurrentMonthTransactionsAsync();
    
    Task<IEnumerable<Transaction>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount);
    
    Task<IEnumerable<Transaction>> GetByCategoryAndDateRangeAsync(Guid categoryId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Transaction>> GetByTypeAndDateRange(TransactionType transactionType, DateTime startDate, DateTime endDate);
    
    Task<IEnumerable<Transaction>> SearchByDescriptionAsync(string searchTerm);
    
    Task<bool> ExistsAsync(Guid id);
    
    Task<decimal> GetTotalByTypeAsync(TransactionType transactionType);
    Task<decimal> GetTotalByTypeAndDateRangeAsync(TransactionType transactionType, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalByCategoryAsync(Guid categoryId);
    Task<decimal> GetBalanceAsync();
    Task<decimal> GetBalanceByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    
    Task<(IEnumerable<Transaction> Transactions, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);

    Task<(IEnumerable<Transaction> Transactions, int TotalCount)> GetPagedWithFiltersAsync(
        int page,
        int pageSize,
        Guid? categoryId = null,
        TransactionType? transactionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? searchTerm = null);
    
    Task<int> CountAsync();
    Task<int> CountByTypeAsync(TransactionType transactionType);
    Task<int> CountByCategoryAsync(Guid categoryId);


}
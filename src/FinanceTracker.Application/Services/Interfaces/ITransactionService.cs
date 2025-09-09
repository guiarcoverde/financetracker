using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.DTOs.Transaction;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.Services.Interfaces;

public interface ITransactionService
{
    Task<TransactionDto> GetByIdAsync(Guid id);
    Task<TransactionDto> CreateAsync(CreateTransactionDto createTransactionDto);
    Task<TransactionDto> UpdateAsync(Guid id, UpdateTransactionDto updateTransactionDto);
    Task DeleteAsync(Guid id);
    
    Task<PagedResultDto<TransactionDto>> GetPagedAsync(TransactionFilterDto filter);
    Task<IEnumerable<TransactionSummaryDto>> GetAllSummariesAsync();
    Task<IEnumerable<TransactionDto>> GetByCategoryIdAsync(Guid categoryId);
    Task<IEnumerable<TransactionDto>> GetByTransactionTypeAsync(TransactionType transactionType);
    
    Task<IEnumerable<TransactionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<TransactionDto>> GetByMonthAsync(int month, int year);
    Task<IEnumerable<TransactionDto>> GetByYearAsync(int year);
    Task<IEnumerable<TransactionDto>> GetTodayTransactionsAsync();
    Task<IEnumerable<TransactionDto>> GetCurrentMonthTransactionsAsync();
    
    Task<IEnumerable<TransactionDto>> SearchAsync(string searchTerm);
    Task<PagedResultDto<TransactionDto>> SearchPagedAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task<bool> ExistsAsync(Guid id);
    Task ValidateTransactionAsync(CreateTransactionDto createTransactionDto);
    Task ValidateTransactionAsync(Guid id,UpdateTransactionDto updateTransactionDto);
    
    Task<decimal> GetTotalByTypeAsync(TransactionType transactionType);
    Task<decimal> GetTotalByTypeAndPeriodAsync(TransactionType transactionType, DateTime startDate, DateTime endDate);
    Task<decimal> GetBalanceAsync();    
    Task<decimal> GetBalanceByPeriodAsync(DateTime startDate, DateTime endDate);
    Task<int> GetTotalCountAsync();
    Task<int> GetCountByTypeAsync(TransactionType transactionType);
}   
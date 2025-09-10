using FinanceTracker.Application.DTOs.Category;
using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.DTOs.Transaction;
using FinanceTracker.Application.Services.Interfaces;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Domain.ValueObjects;
using Mapster;

namespace FinanceTracker.Application.Services.Implementations;

public class TransactionService(IUnitOfWork unitOfWork) : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    
    public async Task<TransactionDto> GetByIdAsync(Guid id)
    {
        var transaction = await _unitOfWork.Transactions.GetByIdAsync(id);
        
        if (transaction == null)
        {
            throw new DomainException($"Transaction with ID {id} not found.");
        }
        
        return MapToTransactionDto(transaction);
    }

    public async Task<TransactionDto> CreateAsync(CreateTransactionDto createDto)
    {
        await ValidateTransactionAsync(createDto);
        
        var category = await _unitOfWork.Categories.GetByIdAsync(createDto.CategoryId);
        if (category == null)
            throw new DomainException($"Categoria com ID {createDto.CategoryId} não foi encontrada.");
        
        var money = new Money(createDto.Amount);
        
        var transaction = new Transaction(createDto.Description, money, createDto.TransactionDate, category);
        var createdTransaction = await _unitOfWork.Transactions.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToTransactionDto(createdTransaction);
    }

    public async Task<TransactionDto> UpdateAsync(Guid id, UpdateTransactionDto updateDto)
    {
        await ValidateTransactionAsync(id, updateDto);

        var transaction = await _unitOfWork.Transactions.GetByIdWithCategoryAsync(id);
        if (transaction == null)
            throw new DomainException($"Transação com ID {id} não foi encontrada.");
        
        var category = await _unitOfWork.Categories.GetByIdAsync(updateDto.CategoryId);
        if (category == null)
            throw new DomainException($"Categoria com ID {updateDto.CategoryId} não foi encontrada.");
        
        transaction.UpdateDescription(updateDto.Description);
        transaction.UpdateAmount(new Money(updateDto.Amount));
        transaction.UpdateTransactionDate(updateDto.TransactionDate);
        transaction.UpdateCategory(category);
        
        await _unitOfWork.Transactions.UpdateAsync(transaction);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToTransactionDto(transaction);
    }

    public async Task DeleteAsync(Guid id)
    {
        if (!await ExistsAsync(id))
            throw new DomainException($"Transação com ID {id} não foi encontrada.");
        
        await _unitOfWork.Transactions.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PagedResultDto<TransactionDto>> GetPagedAsync(TransactionFilterDto filter)
    {
        ValidateFilter(filter);

        var (transactions, totalCount) = await _unitOfWork.Transactions.GetPagedWithFiltersAsync(
            filter.Page,
            filter.PageSize,
            filter.CategoryId,
            filter.TransactionType,
            filter.StartDate,
            filter.EndDate,
            filter.SearchTerm);
        
        var transactionDtos = transactions.Select(MapToTransactionDto).ToList();
        
        return new PagedResultDto<TransactionDto>
        {
            Items = transactionDtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<IEnumerable<TransactionSummaryDto>> GetAllSummariesAsync()
    {
        var transactions = await _unitOfWork.Transactions.GetAllWithCategoriesAsync();
        return transactions.Select(MapToTransactionSummaryDto).ToList();
    }

    public async Task<IEnumerable<TransactionDto>> GetByCategoryIdAsync(Guid categoryId)
    {
        var transactions = await _unitOfWork.Transactions.GetByCategoryIdAsync(categoryId);
        return transactions.Select(MapToTransactionDto).ToList();
    }

    public async Task<IEnumerable<TransactionDto>> GetByTransactionTypeAsync(TransactionType transactionType)
    {
        var transactions = await _unitOfWork.Transactions.GetByTransactionTypeAsync(transactionType);
        return transactions.Select(MapToTransactionDto).ToList();
    }

    public async Task<IEnumerable<TransactionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var transactions = await _unitOfWork.Transactions.GetByDateRangeAsync(startDate, endDate);
        return transactions.Select(MapToTransactionDto).ToList();
    }

    public async Task<IEnumerable<TransactionDto>> GetByMonthAsync(int month, int year)
    {
        ValidateMonthYear(month, year);
        
        var transactions = await _unitOfWork.Transactions.GetByMonthAsync(month, year);
        return transactions.Select(MapToTransactionDto).ToList();
    }

    public async Task<IEnumerable<TransactionDto>> GetByYearAsync(int year)
    {
        ValidateYear(year);
        
        var transactions = await _unitOfWork.Transactions.GetByYearAsync(year);
        return transactions.Select(MapToTransactionDto).ToList();
    }

    public async Task<IEnumerable<TransactionDto>> GetTodayTransactionsAsync()
    {
        var transactions = await _unitOfWork.Transactions.GetTodayTransactionsAsync();
        return transactions.Select(MapToTransactionDto).ToList();
    }

    public async Task<IEnumerable<TransactionDto>> GetCurrentMonthTransactionsAsync()
    {
        var transactions = await _unitOfWork.Transactions.GetCurrentMonthTransactionsAsync();
        return transactions.Select(MapToTransactionDto).ToList();
    }

    public async Task<IEnumerable<TransactionDto>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            throw new DomainException("Termo de busca é obrigatório.");
        
        var transactions = await _unitOfWork.Transactions.SearchByDescriptionAsync(searchTerm.Trim());
        return transactions.Select(MapToTransactionDto).ToList();
    }

    public async Task<PagedResultDto<TransactionDto>> SearchPagedAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            throw new DomainException("Termo de busca é obrigatório.");

        var filter = new TransactionFilterDto()
        {
            SearchTerm = searchTerm.Trim(),
            Page = page,
            PageSize = pageSize
        };
        
        return await GetPagedAsync(filter);
    }

    public async Task<bool> ExistsAsync(Guid id) 
        => await _unitOfWork.Transactions.ExistsAsync(id);

    public async Task ValidateTransactionAsync(CreateTransactionDto createDto)
    {
        if (createDto is null)
            throw new ArgumentNullException(nameof(createDto));
        
        ValidateCommonFields(createDto.Description, createDto.Amount, createDto.TransactionDate);

        if (createDto.CategoryId == Guid.Empty)
            throw new DomainException("A categoria é obrigatória.");
        
        var categoryExists = await _unitOfWork.Categories.ExistsAsync(createDto.CategoryId);
        if (!categoryExists)
            throw new DomainException($"Categoria com ID {createDto.CategoryId} não foi encontrada.");
    }

    public async Task ValidateTransactionAsync(Guid id, UpdateTransactionDto updateDto)
    {
        if (updateDto is null)
            throw new ArgumentNullException(nameof(updateDto));
        
        ValidateCommonFields(updateDto.Description, updateDto.Amount, updateDto.TransactionDate);
        
        if (updateDto.CategoryId == Guid.Empty)
            throw new DomainException("A categoria é obrigatória.");
        
        var transactionExists = await _unitOfWork.Transactions.ExistsAsync(id);
        if (!transactionExists)
            throw new DomainException($"Transação com ID {id} não foi encontrada.");
        
        var categoryExists = await _unitOfWork.Categories.ExistsAsync(updateDto.CategoryId);
        if (!categoryExists)
            throw new DomainException($"Categoria com ID {updateDto.CategoryId} não foi encontrada.");
    }

    public async Task<decimal> GetTotalByTypeAsync(TransactionType transactionType) 
        => await _unitOfWork.Transactions.GetTotalByTypeAsync(transactionType);

    public async Task<decimal> GetTotalByTypeAndPeriodAsync(TransactionType transactionType, DateTime startDate, DateTime endDate)
    {
        ValidateDateRange(startDate, endDate);
        
        return await _unitOfWork.Transactions.GetTotalByTypeAndDateRangeAsync(transactionType, startDate, endDate);
    }

    public async Task<decimal> GetBalanceAsync() 
        => await _unitOfWork.Transactions.GetBalanceAsync();

    public async Task<decimal> GetBalanceByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        ValidateDateRange(startDate, endDate);
        
        return await _unitOfWork.Transactions.GetBalanceByDateRangeAsync(startDate, endDate);
    }

    public async Task<int> GetTotalCountAsync()
        => await _unitOfWork.Transactions.CountAsync();

    public async Task<int> GetCountByTypeAsync(TransactionType transactionType) 
        => await _unitOfWork.Transactions.CountByTypeAsync(transactionType);
    
    private static TransactionDto MapToTransactionDto(Transaction transaction)
    {
        var dto = transaction.Adapt<TransactionDto>();
        
        dto.Amount = transaction.Amount.Amount;
        dto.FormattedAmount = transaction.Amount.ToString();
        dto.FormattedDate = transaction.TransactionDate.ToString("dd/MM/yyyy");
        dto.IsIncome = transaction.IsIncome;
        dto.IsExpense = transaction.IsExpense;
        dto.TransactionType = transaction.TransactionType;

        if (transaction.Category is not null)
        {
            dto.Category = transaction.Category.Adapt<CategorySummaryDto>();
        }

        return dto;
    }

    private static TransactionSummaryDto MapToTransactionSummaryDto(Transaction transaction)
    {
        return new TransactionSummaryDto
        {
            Id = transaction.Id,
            Description = transaction.Description,
            Amount = transaction.Amount.Amount,
            FormattedAmount = transaction.Amount.ToString(),
            TransactionDate = transaction.TransactionDate,
            CategoryName = transaction.Category?.Name ?? "Sem categoria",
            TransactionType = transaction.TransactionType
        };
    }
    
    private static void ValidateCommonFields(string description, decimal amount, DateTime transactionDate)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("A descrição é obrigatória.");
        
        if (amount <= 0)
            throw new DomainException("O valor da transação deve ser maior que zero.");
        
        if (transactionDate == default)
            throw new DomainException("A data da transação é obrigatória.");
    }
    
    private static void ValidateFilter(TransactionFilterDto filter)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        if (filter.Page <= 0)
            throw new DomainException("A página deve ser maior que zero.");

        if (filter.PageSize <= 0 || filter.PageSize > 100)
            throw new DomainException("O tamanho da página deve estar entre 1 e 100.");

        if (filter.StartDate.HasValue && filter.EndDate.HasValue)
            ValidateDateRange(filter.StartDate.Value, filter.EndDate.Value);

        if (filter.MinAmount.HasValue && filter.MaxAmount.HasValue && filter.MinAmount > filter.MaxAmount)
            throw new DomainException("O valor mínimo não pode ser maior que o valor máximo.");
    }
    
    private static void ValidateDateRange(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            throw new DomainException("A data inicial não pode ser maior que a data final.");

        if (startDate > DateTime.Today)
            throw new DomainException("A data inicial não pode ser futura.");
    }
    
    private static void ValidateMonthYear(int month, int year)
    {
        if (month < 1 || month > 12)
            throw new DomainException("O mês deve estar entre 1 e 12.");
        
        ValidateYear(year);
    }
    
    private static void ValidateYear(int year)
    {
        var currentYear = DateTime.Now.Year;
        if (year < currentYear - 10 || year > currentYear)
            throw new DomainException($"Ano deve estar entre {currentYear - 10} e {currentYear}.");
    }
}
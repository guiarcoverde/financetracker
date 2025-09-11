using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FinanceTracker.Infrastructure.Repositories;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;
    
    private ICategoryRepository? _categoryRepository;
    private ITransactionRepository? _transactionRepository;

    public ICategoryRepository Categories
    {
        get
        {
            return _categoryRepository ??= new CategoryRepository(_context);
        }
    }

    public ITransactionRepository Transactions
    {
        get
        {
            return _transactionRepository ??= new TransactionRepository(_context);
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao salvar alterações no banco de dados.", ex);
        }
    }

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("Uma transação já está ativa.");
        }
        
        _currentTransaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("Nenhuma transação ativa para fazer commit.");
        }

        try
        {
            await _context.SaveChangesAsync();
            await _currentTransaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("Nenhuma transação ativa para fazer rollback.");
        }

        try
        {
            await _currentTransaction.RollbackAsync();
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        if (_currentTransaction != null)
            return await operation();
        
        await BeginTransactionAsync();

        try
        {
            var result = await operation();
            await CommitTransactionAsync();
            return result;
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        if (_currentTransaction != null)
        {
            await operation();
            return;
        }
        
        await BeginTransactionAsync();
        
        try
        {
            await operation();
            await CommitTransactionAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
    }
    
    public bool HasActiveTransaction => _currentTransaction != null;

    public async Task<TResult> ExecuteInTransactionWithRetryAsync<TResult>(
        Func<Task<TResult>> operation,
        int maxRetries = 3)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        var attempt = 0;

        while (attempt < maxRetries)
        {
            try
            {
                return await ExecuteInTransactionAsync(operation);
            }
            catch (Exception ex) when (attempt < maxRetries - 1 && IsRetryableException(ex))
            {
                attempt++;
                await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));
            }
        }
        
        return await ExecuteInTransactionAsync(operation);
    }

    public async Task<int> ExecuteBatchAsync(IEnumerable<Func<Task>> operations)
    {
        if (operations == null)
            throw new ArgumentNullException(nameof(operations));
        
        var operationsList = operations.ToList();
        if (!operationsList.Any())
            return 0;

        return await ExecuteInTransactionAsync(async () =>
        {
            foreach(var operation in operationsList)
            {
                await operation();
            }
            return await SaveChangesAsync();
        });
    }

    public void DetachAllEntities()
    {
        var entries = _context.ChangeTracker.Entries().ToList();
        foreach (var entry in entries)
        {
            entry.State = EntityState.Detached;
        }
    }
    
    public int PendingChanges => _context.ChangeTracker.Entries()
        .Count(e => e.State == EntityState.Added ||
                    e.State == EntityState.Modified ||
                    e.State == EntityState.Deleted);
    
    public bool HasPendingChanges => PendingChanges > 0;
    
    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    private static bool IsRetryableException(Exception ex)
        => ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
           ex.Message.Contains("deadlock", StringComparison.OrdinalIgnoreCase) ||
           ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
           ex.GetType().Name.Contains("Timeout", StringComparison.OrdinalIgnoreCase) ||
           ex.GetType().Name.Contains("Connection", StringComparison.OrdinalIgnoreCase);
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            if (_currentTransaction != null)
            {
                try
                {
                    _currentTransaction.Rollback();
                }
                catch
                {
                    // Silenciar exceções durante dispose
                }
                finally
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
        
        _context?.Dispose();
        _disposed = true;
    }
    
    ~UnitOfWork()
    {
        Dispose(false);
    }
}
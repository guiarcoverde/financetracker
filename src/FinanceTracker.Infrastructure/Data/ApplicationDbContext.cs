using System.Text;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FinanceTracker.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        ConfigureGlobalConventions(modelBuilder);
        ConfigureValueConverters(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=finance_tracker_dev;Username=postgres;Password=1234");
        }

        optionsBuilder
            .EnableSensitiveDataLogging(false)
            .EnableDetailedErrors(false)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
    }

    private static void ConfigureGlobalConventions(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName()));
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }

            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName()));
            }
            
            foreach(var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()));
            }
            
            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(ToSnakeCase(foreignKey.GetConstraintName()));
            }
        }

        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(e => e.GetProperties())
                     .Where(p => p.ClrType == typeof(string)))
        {
            if (property.GetMaxLength() == null)
            {
                property.SetMaxLength(256);
            }
        }
        
        foreach(var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(e => e.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,2)");
        }
        
        foreach(var property in modelBuilder.Model.GetEntityTypes()
                    .SelectMany(e => e.GetProperties())
                    .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
        {
            property.SetColumnType("timestamp with time zone");
        }
    }
    
    
    private static void ConfigureValueConverters(ModelBuilder modelBuilder)
    {
        var moneyConverter = new ValueConverter<Money,decimal>(
            v => v.Amount,
            v => new Money(v)
        );

        var moneyComparer = new ValueComparer<Money>(
            (l, r) => l != null && r != null && l.Amount == r.Amount,
            v => v == null ? 0 : v.Amount.GetHashCode(),
            v => v == null ? null : new Money(v.Amount)
        );

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(Money))
                {
                    property.SetValueConverter(moneyConverter);
                    property.SetValueComparer(moneyComparer);
                    property.SetColumnType("decimal(18,2)");
                }
            }
        }
        
        var categoryTypeConverter = new ValueConverter<CategoryType, int>(
            v => (int)v,
            v => (CategoryType)v
            );
        
        var transactionTypeConverter = new ValueConverter<TransactionType, int>(
            v => (int)v,
            v => (TransactionType)v
        );
        
        foreach(var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach(var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(CategoryType))
                {
                    property.SetValueConverter(categoryTypeConverter);
                } else if (property.ClrType == typeof(TransactionType))
                {
                    property.SetValueConverter(transactionTypeConverter);
                }
            }
        }
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added && entry.Property("CreatedAt") != null)
            {
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified && entry.Property("ModifiedAt") != null)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }
    }

    private static string ToSnakeCase(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input ?? string.Empty;

        var result = new StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
            {
                if (i > 0 && !char.IsUpper(input[i - 1]))
                {
                    result.Append('_');
                }
                result.Append(char.ToLower(input[i]));
            }
            else
            {
                result.Append(input[i]);
            }
        }
        
        return result.ToString();
    }
}
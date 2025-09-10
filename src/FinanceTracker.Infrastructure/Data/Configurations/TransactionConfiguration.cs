using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
        public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .IsRequired()
            .ValueGeneratedNever(); 

        
        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired();

        
        builder.Property(t => t.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired()
            .HasConversion(
                v => v.Amount,              
                v => new Money(v),          
                new ValueComparer<Money>(
                    (l, r) => l != null && r != null && l.Amount == r.Amount,
                    v => v == null ? 0 : v.Amount.GetHashCode(),
                    v => v == null ? null : new Money(v.Amount)
                )
            );

        builder.Property(t => t.TransactionDate)
            .HasColumnName("transaction_date")
            .HasColumnType("timestamp with time zone") 
            .IsRequired();

        
        builder.Property(t => t.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        
        ConfigureRelationships(builder);

        
        ConfigureIndexes(builder);

        
        ConfigureConstraints(builder);

        
        ConfigureQueryFilters(builder);
    }

    private static void ConfigureRelationships(EntityTypeBuilder<Transaction> builder)
    {
        
        builder.HasOne(t => t.Category)
            .WithMany() 
            .HasForeignKey(t => t.CategoryId)
            .HasConstraintName("fk_transactions_category_id")
            .OnDelete(DeleteBehavior.Restrict); 

        
        builder.Navigation(t => t.Category)
            .EnableLazyLoading(false); 
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasIndex(t => t.CategoryId)
            .HasDatabaseName("ix_transactions_category_id");

        
        builder.HasIndex(t => t.TransactionDate)
            .HasDatabaseName("ix_transactions_transaction_date");

        
        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("ix_transactions_created_at");

        
        builder.HasIndex(t => t.UpdatedAt)
            .HasDatabaseName("ix_transactions_updated_at");

        
        builder.HasIndex(t => new { t.CategoryId, t.TransactionDate })
            .HasDatabaseName("ix_transactions_category_date");

        
        builder.HasIndex(t => new { t.TransactionDate, t.Amount })
            .HasDatabaseName("ix_transactions_date_amount");

      
        builder.HasIndex(t => t.Description)
            .HasDatabaseName("ix_transactions_description");
    }

    private static void ConfigureConstraints(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasCheckConstraint(
            "ck_transactions_amount_positive", 
            "amount > 0");

      
        builder.HasCheckConstraint(
            "ck_transactions_date_not_future", 
            "transaction_date <= CURRENT_DATE");

       
        builder.HasCheckConstraint(
            "ck_transactions_description_not_empty", 
            "LENGTH(TRIM(description)) >= 3");
    }

    private static void ConfigureQueryFilters(EntityTypeBuilder<Transaction> builder)
    {   
        //TODO:
        // Filtros globais de consulta - futuras implementações
        // Exemplo: soft delete
        // builder.HasQueryFilter(t => !t.IsDeleted);
        
        // Exemplo: filtro por tenant (multi-tenancy)
        // builder.HasQueryFilter(t => t.TenantId == CurrentTenant.Id);
    }
}
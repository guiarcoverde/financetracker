using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Data.Configurations;

public class CategoryConfigurations : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .IsRequired()
            .ValueGeneratedNever();
        builder.Property(c=> c.CategoryType)
            .HasColumnName("category_type")
            .HasConversion<int>()
            .IsRequired();
        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("NOW()");
        
        
        builder.HasIndex(c => c.Name)
            .HasDatabaseName("ix_categories_name")
            .IsUnique();
        builder.HasIndex(c => c.CategoryType)
            .HasDatabaseName("ix_categories_category_type");
        builder.HasIndex(c => c.CreatedAt)
            .HasDatabaseName("ix_categories_created_at");
        
        SeedData(builder);
    }
    
    private static void ConfigureRelationships(EntityTypeBuilder<Category> builder)
    {
        // Relacionamento com Transactions (One-to-Many)
        // A navegação será configurada em TransactionConfiguration para evitar duplicação
    }

    private static void ConfigureQueryFilters(EntityTypeBuilder<Category> builder)
    {
        //TODO: Filtros globais de consulta (soft delete, etc.) - futuro
        // builder.HasQueryFilter(c => !c.IsDeleted);
    }

    private static void SeedData(EntityTypeBuilder<Category> builder)
    {
        var seedCategories = new[]
        {
            // Categorias de Despesa
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Alimentação",
                CategoryType = CategoryType.Food,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111112"),
                Name = "Saúde",
                CategoryType = CategoryType.Health,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111113"),
                Name = "Transporte",
                CategoryType = CategoryType.Transport,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111114"),
                Name = "Lazer",
                CategoryType = CategoryType.Entertainment,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111115"),
                Name = "Moradia",
                CategoryType = CategoryType.Housing,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },

            // Categorias de Receita
            new
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222221"),
                Name = "Salário",
                CategoryType = CategoryType.Salary,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Freelance",
                CategoryType = CategoryType.Freelance,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222223"),
                Name = "Investimentos",
                CategoryType = CategoryType.Investment,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };
        
        builder.HasData(seedCategories);
    }
    
    

}
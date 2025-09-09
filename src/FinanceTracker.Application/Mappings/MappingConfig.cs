using FinanceTracker.Application.DTOs.Category;
using FinanceTracker.Domain.Entities;
using Mapster;

namespace FinanceTracker.Application.Mappings;

public static class MappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Category, CategoryDto>
            .NewConfig()
            .Map(dest => dest.DisplayName, src => src.DisplayName)
            .Map(dest => dest.IsExpenseCategory, src => src.IsExpenseCategory)
            .Map(dest => dest.IsIncomeCategory, src => src.IsIncomeCategory)
            .Map(dest => dest.TransactionType, src => src.TransactionType);
    }
}
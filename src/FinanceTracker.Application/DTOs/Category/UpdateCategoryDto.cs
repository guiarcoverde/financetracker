using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.DTOs.Category;

public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public CategoryType CategoryType { get; set; }
}
namespace FinanceTracker.Application.DTOs.Dashboard;

public class CategoryBudgetDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal BudgetedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance => ActualAmount - BudgetedAmount;
    public decimal PercentageUsed => BudgetedAmount > 0 ? (ActualAmount / BudgetedAmount) * 100 : 0;

    public bool IsOverBudget => Variance > 0;
    public string Status => IsOverBudget ? "Acima do orçamento" : "Dentro do orçamento";
}
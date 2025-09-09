namespace FinanceTracker.Application.DTOs.Dashboard;

public class BudgetAnalysisDto
{
    public decimal BudgetedIncome { get; set; }
    public decimal BudgetedExpenses { get; set; }
    public decimal ActualIncome { get; set; }
    public decimal ActualExpenses { get; set; }
    
    public decimal IncomeVariance => ActualIncome - BudgetedIncome;
    public decimal ExpenseVariance => ActualExpenses - BudgetedExpenses;
    public decimal BudgetPerformance => (ActualIncome - ActualExpenses) - (BudgetedIncome - BudgetedExpenses);

    public bool IsOverBudget => ExpenseVariance > 0;
    public bool IsUnderBudget => ExpenseVariance < 0;
    public bool MetIncomeGoal => IncomeVariance >= 0;
    
    public IEnumerable<CategoryBudgetDto> CategoryAnalysis { get; set; } = new List<CategoryBudgetDto>();
}
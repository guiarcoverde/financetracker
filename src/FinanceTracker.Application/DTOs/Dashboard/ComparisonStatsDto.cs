namespace FinanceTracker.Application.DTOs.Dashboard;

public class ComparisonStatsDto
{
    public FinancialSummaryDto CurrentPeriod { get; set; } = new();
    public FinancialSummaryDto PreviousPeriod { get; set; } = new();
    
    public decimal IncomeVariationPercentage { get; set; }
    public decimal ExpenseVariationPercentage { get; set; }
    public decimal BalanceVariationPercentage { get; set; }
    
    public decimal IncomeVariationAmount { get; set; }
    public decimal ExpenseVariationAmount { get; set; }
    public decimal BalanceVariationAmount { get; set; }
    
    public bool IncomeImproved => IncomeVariationPercentage > 0;
    public bool ExpenseImproved => ExpenseVariationPercentage < 0;
    public bool BalanceImproved => BalanceVariationPercentage > 0;
    
    public string PeriodDescription { get; set; } = string.Empty;
}
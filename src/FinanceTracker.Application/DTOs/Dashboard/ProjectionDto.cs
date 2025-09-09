namespace FinanceTracker.Application.DTOs.Dashboard;

public class ProjectionDto
{
    public DateTime ProjectionDate { get; set; }
    public decimal ProjectedIncome { get; set; }
    public decimal ProjectedExpenses { get; set; }
    public decimal ProjectedBalance { get; set; }
    
    public string FormattedProjectedIncome { get; set; } = string.Empty;
    public string FormattedProjectedExpenses { get; set; } = string.Empty;
    public string FormattedProjectedBalance { get; set; } = string.Empty;
    
    public string ProjectionMethod { get; set; } = string.Empty;
    public int DataPointsUsed { get; set; }
    public decimal ConfidenceLevel { get; set; }
}
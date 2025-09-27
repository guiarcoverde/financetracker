using FinanceTracker.Application.DTOs.Dashboard;
using FinanceTracker.Application.Services.Interfaces;
using FinanceTracker.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtém o dashboard completo com todos os dados
    /// </summary>
    /// <returns>Dashboard completo</returns>
    /// <response code="200">Retorna os dados completos do dashboard</response>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        _logger.LogInformation("Carregando dashboard completo");
        var dashboard = await _dashboardService.GetDashboardAsync();
        
        _logger.LogInformation("Dashboard carregado - Saldo atual: {CurrentBalance}, Transações recentes: {RecentCount}", 
            dashboard.CurrentMonth.FormattedBalance, dashboard.RecentTransactions.Count());
        return Ok(dashboard);
    }

    /// <summary>
    /// Obtém dashboard personalizado para um período específico
    /// </summary>
    /// <param name="startDate">Data inicial (yyyy-MM-dd)</param>
    /// <param name="endDate">Data final (yyyy-MM-dd)</param>
    /// <returns>Dashboard para o período especificado</returns>
    /// <response code="200">Retorna os dados do dashboard para o período</response>
    /// <response code="400">Período inválido</response>
    [HttpGet("period")]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DashboardDto>> GetDashboardByPeriod(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest(new { message = "A data inicial não pode ser maior que a data final" });
        }

        if (startDate > DateTime.Today)
        {
            return BadRequest(new { message = "A data inicial não pode ser futura" });
        }

        _logger.LogInformation("Carregando dashboard para período: {StartDate} a {EndDate}",
            startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

        var dashboard = await _dashboardService.GetDashboardForPeriodAsync(startDate, endDate);

        return Ok(dashboard);
    }

    /// <summary>
    /// Obtém resumo financeiro do mês atual
    /// </summary>
    /// <returns>Resumo financeiro do mês atual</returns>
    /// <response code="200">Retorna o resumo do mês atual</response>
    [HttpGet("summary/current-month")]
    [ProducesResponseType(typeof(FinancialSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<FinancialSummaryDto>> GetCurrentMonthSummary()
    {
        _logger.LogInformation("Carregando resumo do mês atual");
        var summary = await _dashboardService.GetCurrentMonthSummaryAsync();

        _logger.LogInformation("Resumo do mês atual - Receitas: {Income}, Despesas: {Expenses}, Saldo: {Balance}",
            summary.FormattedTotalIncome, summary.FormattedTotalExpenses, summary.FormattedBalance);
        return Ok(summary);
    }

    /// <summary>
    /// Obtém resumo financeiro do mês anterior
    /// </summary>
    /// <returns>Resumo financeiro do mês anterior</returns>
    /// <response code="200">Retorna o resumo do mês anterior</response>
    [HttpGet("summary/last-month")]
    [ProducesResponseType(typeof(FinancialSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<FinancialSummaryDto>> GetLastMonthSummary()
    {
        _logger.LogInformation("Carregando resumo do mês anterior");
        var summary = await _dashboardService.GetLastMonthSummaryAsync();

        return Ok(summary);
    }
    
    /// <summary>
    /// Obtém resumo financeiro do ano atual
    /// </summary>
    /// <returns>Resumo financeiro do ano atual</returns>
    /// <response code="200">Retorna o resumo do ano atual</response>
    [HttpGet("summary/current-year")]
    [ProducesResponseType(typeof(FinancialSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<FinancialSummaryDto>> GetCurrentYearSummary()
    {
        _logger.LogInformation("Carregando resumo do ano atual");
        var summary = await _dashboardService.GetCurrentYearSummaryAsync();

        return Ok(summary);
    }

    /// <summary>
    /// Obtém resumo financeiro para período personalizado
    /// </summary>
    /// <param name="startDate">Data inicial (yyyy-MM-dd)</param>
    /// <param name="endDate">Data final (yyyy-MM-dd)</param>
    /// <returns>Resumo financeiro do período</returns>
    /// <response code="200">Retorna o resumo do período</response>
    /// <response code="400">Período inválido</response>
    [HttpGet("summary/period")]
    [ProducesResponseType(typeof(FinancialSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FinancialSummaryDto>> GetSummaryForPeriod(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest(new { message = "A data inicial não pode ser maior que a data final" });
        }

        _logger.LogInformation("Carregando resumo para período: {StartDate} a {EndDate}",
            startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

        var summary = await _dashboardService.GetSummaryForPeriodAsync(startDate, endDate);

        return Ok(summary);
    }

    /// <summary>
    /// Obtém estatísticas por categoria
    /// </summary>
    /// <param name="startDate">Data inicial (opcional)</param>
    /// <param name="endDate">Data final (opcional)</param>
    /// <returns>Estatísticas das categorias</returns>
    /// <response code="200">Retorna as estatísticas por categoria</response>
    /// <response code="400">Período inválido</response>
    [HttpGet("category-stats")]
    [ProducesResponseType(typeof(IEnumerable<CategoryStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<CategoryStatsDto>>> GetCategoryStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            return BadRequest(new { message = "A data inicial não pode ser maior que a data final" });
        }

        IEnumerable<CategoryStatsDto> stats;

        if (startDate.HasValue && endDate.HasValue)
        {
            _logger.LogInformation("Carregando estatísticas de categorias para período: {StartDate} a {EndDate}",
                startDate.Value.ToString("yyyy-MM-dd"), endDate.Value.ToString("yyyy-MM-dd"));
            
            stats = await _dashboardService.GetCategoryStatsForPeriodAsync(startDate.Value, endDate.Value);
        }
        else
        {
            _logger.LogInformation("Carregando estatísticas de categorias do mês atual");
            stats = await _dashboardService.GetCategoryStatsAsync();
        }
        
        _logger.LogInformation("Carregadas estatísticas de {Count} categorias", stats.Count());
        return Ok(stats);
    }

    /// <summary>
    /// Obtém top categorias por despesas
    /// </summary>
    /// <param name="limit">Número máximo de categorias (padrão: 5, máximo: 20)</param>
    /// <returns>Top categorias por despesas</returns>
    /// <response code="200">Retorna as principais categorias de despesa</response>
    /// <response code="400">Limite inválido</response>
    [HttpGet("top-expenses")]
    [ProducesResponseType(typeof(IEnumerable<CategoryStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<CategoryStatsDto>>> GetTopExpenseCategories(
        [FromQuery] int limit = 5)
    {
        if (limit <= 0 || limit > 20)
        {
            return BadRequest(new { message = "O limite deve ser entre 1 e 20" });
        }
        
        _logger.LogInformation("Carregando top {Limit} categorias de despesa", limit);
        var categories = await _dashboardService.GetTopCategoriesByExpenseAsync(limit);
        
        return Ok(categories);
    }
    
    /// <summary>
    /// Obtém top categorias por receitas
    /// </summary>
    /// <param name="limit">Número máximo de categorias (padrão: 5, máximo: 20)</param>
    /// <returns>Top categorias por receitas</returns>
    /// <response code="200">Retorna as principais categorias de receita</response>
    /// <response code="400">Limite inválido</response>
    [HttpGet("top-incomes")]
    [ProducesResponseType(typeof(IEnumerable<CategoryStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<CategoryStatsDto>>> GetTopIncomeCategories(
        [FromQuery] int limit = 5)
    {
        if (limit <= 0 || limit > 20)
        {
            return BadRequest(new { message = "Limite deve estar entre 1 e 20" });
        }

        _logger.LogInformation("Carregando top {Limit} categorias de receita", limit);
        var categories = await _dashboardService.GetTopCategoriesByIncomeAsync(limit);

        return Ok(categories);
    }

    /// <summary>
    /// Obtém tendência mensal
    /// </summary>
    /// <param name="months">Número de meses (padrão: 6, máximo: 24)</param>
    /// <returns>Dados da tendência mensal</returns>
    /// <response code="200">Retorna a tendência mensal</response>
    /// <response code="400">Número de meses inválido</response>
    [HttpGet("trends/monthly")]
    [ProducesResponseType(typeof(IEnumerable<MonthlyStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<MonthlyStatsDto>>> GetMonthlyTrends([FromQuery] int months = 6)
    {
        if (months is <= 0 or > 24)
        {
            return BadRequest(new { message = "O número de meses deve estar entre 1 e 24" });
        }
        
        _logger.LogInformation("Carregando tendência mensal para os últimos {Months} meses", months);
        var trends = await _dashboardService.GetMonthlyTrendAsync(months);
        return Ok(trends);
    }

    /// <summary>
    /// Obtém transações recentes
    /// </summary>
    /// <param name="limit">Número máximo de transações (padrão: 10, máximo: 50)</param>
    /// <returns>Lista das transações recentes</returns>
    /// <response code="200">Retorna as transações recentes</response>
    /// <response code="400">Limite inválido</response>
    [HttpGet("recent-transactions")]
    [ProducesResponseType(typeof(IEnumerable<RecentTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<RecentTransactionDto>>> GetRecentTransactions(
        [FromQuery] int limit = 10)
    {
        if (limit <= 0 || limit > 50)
        {
            return BadRequest(new { message = "O limite deve estar entre 1 e 50" });
        }

        _logger.LogInformation("Carregando {Limit} transações recentes", limit);
        var transactions = await _dashboardService.GetRecentTransactionsAsync(limit);
        return Ok(transactions);
    }
    
    /// <summary>
    /// Obtém comparação mês atual vs mês anterior
    /// </summary>
    /// <returns>Dados da comparação mensal</returns>
    /// <response code="200">Retorna a comparação mensal</response>
    [HttpGet("comparison/month-over-month")]
    [ProducesResponseType(typeof(ComparisonStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ComparisonStatsDto>> GetMonthOverMonthComparison()
    {
        _logger.LogInformation("Carregando comparação mês atual vs mês anterior");
        var comparison = await _dashboardService.GetMonthOverMonthComparisonAsync();

        _logger.LogInformation("Comparação mensal - Variação de receitas: {IncomeVariation}%, Variação de despesas: {ExpenseVariation}%",
            comparison.IncomeVariationPercentage, comparison.ExpenseVariationPercentage);

        return Ok(comparison);
    }

    /// <summary>
    /// Obtém comparação ano atual vs ano anterior
    /// </summary>
    /// <returns>Dados da comparação anual</returns>
    /// <response code="200">Retorna a comparação anual</response>
    [HttpGet("comparison/year-over-year")]
    [ProducesResponseType(typeof(ComparisonStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ComparisonStatsDto>> GetYearOverYearComparison()
    {
        _logger.LogInformation("Carregando comparação ano atual vs ano anterior");
        var comparison = await _dashboardService.GetYearOverYearComparisonAsync();

        return Ok(comparison);
    }

    /// <summary>
    /// Obtém projeções financeiras futuras
    /// </summary>
    /// <returns>Dados das projeções mensais</returns>
    /// <response code="200">Retorna as projeções futuras</response>
    [HttpGet("projections/monthly")]
    [ProducesResponseType(typeof(ProjectionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProjectionDto>> GetMonthlyProjections()
    {
        _logger.LogInformation("Carregando projeções mensais");
        var projection = await _dashboardService.GetMonthlyProjectionAsync();

        _logger.LogInformation("Projeção mensal - Receita: {Income}, Despesa: {Expenses}, Confiança: {Confidence}%",
            projection.FormattedProjectedIncome, projection.FormattedProjectedExpenses, projection.ConfidenceLevel);

        return Ok(projection);
    }
}
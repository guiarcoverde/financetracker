using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.DTOs.Transaction;
using FinanceTracker.Application.Services.Interfaces;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    
    /// <summary>
    /// Obtém transações com paginação e filtros
    /// </summary>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 10, máximo: 100)</param>
    /// <param name="categoryId">ID da categoria para filtrar</param>
    /// <param name="transactionType">Tipo da transação (Income/Expense)</param>
    /// <param name="startDate">Data inicial (yyyy-MM-dd)</param>
    /// <param name="endDate">Data final (yyyy-MM-dd)</param>
    /// <param name="searchTerm">Termo para busca na descrição</param>
    /// <returns>Lista paginada de transações</returns>
    /// <response code="200">Retorna as transações paginadas</response>
    /// <response code="400">Parâmetros de filtro inválidos</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResultDto<TransactionDto>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? transactionType = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? searchTerm = null
        )
    {
        try
        {
            TransactionType? parsedTransactionType = null;
            if (!string.IsNullOrEmpty(transactionType))
            {
                if (!Enum.TryParse<TransactionType>(transactionType, true, out var type))
                {
                    _logger.LogWarning("Tipo de transação inválido: {TransactionType}", transactionType);
                    return BadRequest(new
                        { message = $"Tipo de transação inválido: {transactionType}. Use 'Income' ou 'Expense'" });
                }

                parsedTransactionType = type;
            }

            var filter = new TransactionFilterDto
            {
                Page = page,
                PageSize = pageSize,
                CategoryId = categoryId,
                TransactionType = parsedTransactionType,
                StartDate = startDate,
                EndDate = endDate,
                SearchTerm = searchTerm
            };

            _logger.LogInformation("Buscando transações paginadas: Página {Page}, Tamanho {PageSize}", page, pageSize);
            var result = await _transactionService.GetPagedAsync(filter);

            _logger.LogInformation(
                "Encontradas {TotalCount} transações, retornando página {Page} com {ItemCount} itens",
                result.TotalCount, result.Page, result.Items.Count());

            return Ok(result);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Filtros inválidos para busca de transações");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar transações paginadas");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao buscar transações" });
        }
    }
    
    /// <summary>
    /// Obtém uma transação por ID
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <returns>Dados da transação</returns>
    /// <response code="200">Retorna os dados da transação</response>
    /// <response code="404">Transação não encontrada</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDto>> GetById(Guid id)
    {
        try
        {
            _logger.LogInformation("Buscando transação com ID: {TransactionId}", id);
            var transaction = await _transactionService.GetByIdAsync(id);

            _logger.LogInformation("Transação encontrada: {Description}", transaction.Description);
            return Ok(transaction);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Transação não encontrada: {TransactionId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar transação: {TransactionId}", id);            
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao buscar transação" });
        }
    }

    /// <summary>
    /// Obtém transações por categoria
    /// </summary>
    /// <param name="categoryId">ID da categoria</param>
    /// <returns>Lista de transações da categoria</returns>
    /// <response code="200">Retorna as transações da categoria</response>
    [HttpGet("by-category/{categoryId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetByCategory(Guid categoryId)
    {
        try
        {
            _logger.LogInformation("Buscando transações da categoria: {CategoryId}", categoryId);
            var transactions = await _transactionService.GetByCategoryIdAsync(categoryId);

            _logger.LogInformation("Encontradas {Count} transações da categoria {CategoryId}",
                transactions.Count(), categoryId);

            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar transações por categoria: {CategoryId}", categoryId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao buscar transações por categoria" });
        }
    }

    /// <summary>
    /// Obtém transações por tipo
    /// </summary>
    /// <param name="type">Tipo da transação (Income/Expense)</param>
    /// <returns>Lista de transações do tipo especificado</returns>
    /// <response code="200">Retorna as transações do tipo especificado</response>
    /// <response code="400">Tipo de transação inválido</response>
    [HttpGet("by-type/{type}")]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetByType(string type)
    {
        try
        {
            if (!Enum.TryParse<TransactionType>(type, true, out var transactionType))
            {
                _logger.LogWarning("Tipo de transação inválido: {Type}", type);
                return BadRequest(new { message = $"Tipo de transação inválido: {{type}}. Use 'Income' ou 'Expense'" });
            }

            _logger.LogInformation("Buscando transações do tipo: {TransactionType}", transactionType);
            var transactions = await _transactionService.GetByTransactionTypeAsync(transactionType);

            _logger.LogInformation("Encontradas {Count} transações do tipo {TransactionType}",
                transactions.Count(), transactionType);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar transações por tipo: {Type}", type);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao buscar transações por tipo" });
        }
    }
    
    /// <summary>
    /// Obtém transações de um período específico
    /// </summary>
    /// <param name="startDate">Data inicial (yyyy-MM-dd)</param>
    /// <param name="endDate">Data final (yyyy-MM-dd)</param>
    /// <returns>Lista de transações do período</returns>
    /// <response code="200">Retorna as transações do período</response>
    /// <response code="400">Período inválido</response>
    [HttpGet("by-period")]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetByPeriod(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                return BadRequest(new { message = "A data inicial não pode ser maior que a data final" });
            }

            _logger.LogInformation("Buscando transações do período: {StartDate} a {EndDate}",
                startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
            var transactions = await _transactionService.GetByDateRangeAsync(startDate, endDate);

            _logger.LogInformation("Encontradas {Count} transações no período", transactions.Count());
            return Ok(transactions);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Período inválido: {StartDate} a {EndDate}", startDate, endDate);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar transações por período");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao buscar transações por período" });
        }
    }
    
    /// <summary>
    /// Obtém transações do mês atual
    /// </summary>
    /// <returns>Lista de transações do mês atual</returns>
    /// <response code="200">Retorna as transações do mês atual</response>
    [HttpGet("current-month")]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetCurrentMonth()
    {
        try
        {
            _logger.LogInformation("Buscando transações do mês atual");
            var transactions = await _transactionService.GetCurrentMonthTransactionsAsync();

            _logger.LogInformation("Encontradas {Count} transações do mês atual", transactions.Count());
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar transações do mês atual");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao buscar transações do mês atual" });
        }
    }

    /// <summary>
    /// Obtém transações de hoje
    /// </summary>
    /// <returns>Lista de transações de hoje</returns>
    /// <response code="200">Retorna as transações de hoje</response>
    [HttpGet("today")]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetToday()
    {
        try
        {
            _logger.LogInformation("Buscando transações de hoje");
            var transactions = await _transactionService.GetTodayTransactionsAsync();

            _logger.LogInformation("Encontradas {Count} transações de hoje", transactions.Count());
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar transações de hoje");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao buscar transações de hoje" });
        }
    }

    /// <summary>
    /// Busca transações por texto na descrição
    /// </summary>
    /// <param name="q">Termo de busca</param>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 10)</param>
    /// <returns>Resultado paginado da busca</returns>
    /// <response code="200">Retorna os resultados da busca</response>
    /// <response code="400">Termo de busca inválido</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResultDto<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResultDto<TransactionDto>>> Search(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new { message = "Termo de busca é obrigatório" });
            }

            if (q.Length < 2)
            {
                return BadRequest(new { message = "Termo de busca deve ter pelo menos 2 caracteres" });
            }

            _logger.LogInformation("Buscando transações com termo: {SearchTerm}", q);
            var result = await _transactionService.SearchPagedAsync(q, page, Math.Min(pageSize, 100));

            _logger.LogInformation("Encontradas {TotalCount} transações com o termo '{SearchTerm}'",
                result.TotalCount, q);
            return Ok(result);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Termo de busca inválido: {SearchTerm}", q);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar transações por texto: {SearchTerm}", q);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao buscar transações" });
        }
    }
    
    /// <summary>
    /// Cria uma nova transação
    /// </summary>
    /// <param name="createDto">Dados para criação da transação</param>
    /// <returns>Transação criada</returns>
    /// <response code="201">Transação criada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    [HttpPost("create-transaction")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransactionDto>> Create([FromBody] CreateTransactionDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Dados inválidos para criação de transação: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Criando nova transação: {Description}, Valor: {Amount}",
                createDto.Description, createDto.Amount);

            var transaction = await _transactionService.CreateAsync(createDto);
            _logger.LogInformation("Transação criada com sucesso. ID: {TransactionId}", transaction.Id);

            return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para criação de transação: {Description}",
                createDto?.Description);

            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogWarning(ex, "DTO nulo para criação de transação");
            return BadRequest(new { message = "Dados da transação são obrigatórios" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar transação: {Description}", createDto?.Description);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao criar transação" });
        }
    }

    /// <summary>
    /// Atualiza uma transação existente
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <param name="updateDto">Dados para atualização da transação</param>
    /// <returns>Transação atualizada</returns>
    /// <response code="200">Transação atualizada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Transação não encontrada</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDto>> Update([FromRoute] Guid id, UpdateTransactionDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Atualizando transação: {TransactionId}", id);
            var transaction = await _transactionService.UpdateAsync(id, updateDto);

            _logger.LogInformation("Transação atualizada com sucesso: {TransactionId}", id);
            return Ok(transaction);
        }
        catch (DomainException ex) when (ex.Message.Contains("não foi encontrada"))
        {
            _logger.LogWarning(ex, "Transação não encontrada para atualização: {TransactionId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para atualização da transação: {TransactionId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar transação: {TransactionId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao atualizar transação" });
        }
    }

    /// <summary>
    /// Exclui uma transação
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <returns>Confirmação da exclusão</returns>
    /// <response code="204">Transação excluída com sucesso</response>
    /// <response code="404">Transação não encontrada</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            _logger.LogInformation("Excluindo transação: {TransactionId}", id);
            await _transactionService.DeleteAsync(id);

            _logger.LogInformation("Transação excluída com sucesso: {TransactionId}", id);
            return NoContent();
        }
        catch (DomainException ex) when (ex.Message.Contains("não foi encontrada"))
        {
            _logger.LogWarning(ex, "Transação não encontrada para exclusão: {TransactionId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir transação: {TransactionId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao excluir transação" });
        }
    }
    
    /// <summary>
    /// Obtém estatísticas das transações
    /// </summary>
    /// <returns>Estatísticas das transações</returns>
    /// <response code="200">Retorna as estatísticas</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetStatistics()
    {
        try
        {
            _logger.LogInformation("Buscando estatísticas das transações");
            var totalCount = await _transactionService.GetTotalCountAsync();
            var totalIncome = await _transactionService.GetTotalByTypeAsync(TransactionType.Income);
            var totalExpenses = await _transactionService.GetTotalByTypeAsync(TransactionType.Expense);
            var balance = await _transactionService.GetBalanceAsync();
            var incomeCount = await _transactionService.GetCountByTypeAsync(TransactionType.Income);
            var expenseCount = await _transactionService.GetCountByTypeAsync(TransactionType.Expense);
            
            var stats = new
            {
                TotalCount = totalCount,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                Balance = balance,
                IncomeCount = incomeCount,
                ExpenseCount = expenseCount
            };

            _logger.LogInformation("Estatísticas calculadas: {TotalTransactions} transações, Saldo: {Balance:C}",
                totalCount, balance);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar estatísticas das transações");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao buscar estatísticas" });
        }
    }

    /// <summary>
    /// Obtém estatísticas das transações por período
    /// </summary>
    /// <param name="startDate">Data inicial (yyyy-MM-dd)</param>
    /// <param name="endDate">Data final (yyyy-MM-dd)</param>
    /// <returns>Estatísticas do período</returns>
    /// <response code="200">Retorna as estatísticas do período</response>
    /// <response code="400">Período inválido</response>
    [HttpGet("stats/period")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> GetStatisticsByPeriod(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                return BadRequest(new { message = "A data inicial não pode ser maior que a data final" });
            }

            _logger.LogInformation("Buscando estatísticas do período: {StartDate} a {EndDate}",
                startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            var totalIncome =
                await _transactionService.GetTotalByTypeAndPeriodAsync(TransactionType.Income, startDate, endDate);
            var totalExpenses =
                await _transactionService.GetTotalByTypeAndPeriodAsync(TransactionType.Expense, startDate, endDate);
            var balance = await _transactionService.GetBalanceByPeriodAsync(startDate, endDate);

            var stats = new
            {
                period = new
                {
                    startDate = startDate.ToString("yyyy-MM-dd"),
                    endDate = endDate.ToString("yyyy-MM-dd")
                },
                totalIncome,
                totalExpenses,
                balance,
                formattedTotalIncome = $"R$ {totalIncome:N2}",
                formattedTotalExpenses = $"R$ {totalExpenses:N2}",
                formattedBalance = $"R$ {balance:N2}"
            };

            return Ok(stats);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Período inválido para estatísticas");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar estatísticas por período");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao buscar estatísticas do período" });
        }
    }
}
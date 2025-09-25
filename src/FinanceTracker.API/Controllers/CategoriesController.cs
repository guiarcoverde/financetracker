using FinanceTracker.Application.DTOs.Category;
using FinanceTracker.Application.Services.Interfaces;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;


[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todas as categorias
    /// </summary>
    /// <returns>Lista de todas as categorias</returns>
    /// <response code="200">Retorna a lista de categorias</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        try
        {
            _logger.LogInformation("Buscando todas as categorias");
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Erro ao buscar todas as categorias");

            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao buscar todas as categorias" });
        }
    }

    /// <summary>
    /// Obtém uma categoria por ID
    /// </summary>
    /// <param name="id">ID da categoria</param>
    /// <returns>Dados da categoria</returns>
    /// <response code="200">Retorna os dados da categoria</response>
    /// <response code="404">Categoria não encontrada</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(Guid id)
    {
        try
        {
            _logger.LogInformation("Buscando categoria com ID: {CategoryId}", id);
            var category = await _categoryService.GetByIdAsync(id);
            _logger.LogInformation("Categoria encontrada: {CategoryName}", category.Name);
            return Ok(category);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Categoria não encontrada: {CategoryId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar categoria: {CategoryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao buscar a categoria" });
        }
    }

    /// <summary>
    /// Obtém resumos de todas as categorias (para dropdowns/listas)
    /// </summary>
    /// <returns>Lista de resumos das categorias</returns>
    /// <response code="200">Retorna os resumos das categorias</response>
    [HttpGet("summaries")]
    [ProducesResponseType(typeof(IEnumerable<CategorySummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategorySummaryDto>>> GetCategorySummaries()
    {
        try
        {
            _logger.LogInformation("Buscando resumos de categorias");
            var summaries = await _categoryService.GetSummariesAsync();
            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar resumos de categorias");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao buscar resumos de categorias" });
        }
    }

    /// <summary>
    /// Obtém categorias por tipo de transação
    /// </summary>
    /// <param name="type">Tipo da transação (Income ou Expense)</param>
    /// <returns>Lista de categorias do tipo especificado</returns>
    /// <response code="200">Retorna as categorias do tipo especificado</response>
    /// <response code="400">Tipo de transação inválido</response>
    [HttpGet("by-type/{type}")]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategoriesByType(string type)
    {
        try
        {
            if (!Enum.TryParse<TransactionType>(type, true, out var transactionType))
            {
                _logger.LogWarning("Tipo de transação inválido: {Type}", type);
                return BadRequest(new { message = "Tipo de transação inválido. Use 'Income' ou 'Expense'." });
            }

            _logger.LogInformation("Buscando categorias do tipo: {Type}", transactionType);
            var categories = await _categoryService.GetByTransactionTypeAsync(transactionType);

            _logger.LogInformation("Encontradas {Count} categorias do tipo {TransactionType}",
                categories.Count(), transactionType);

            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar categorias do tipo: {Type}", type);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao buscar categorias por tipo" });
        }
    }

    /// <summary>
    /// Obtém apenas categorias de despesa
    /// </summary>
    /// <returns>Lista de categorias de despesa</returns>
    /// <response code="200">Retorna as categorias de despesa</response>
    [HttpGet("expenses")]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetExpenseCategories()
    {
        try
        {
            _logger.LogInformation("Buscando categorias de despesa");
            var categories = await _categoryService.GetExpenseCategoriesAsync();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar categorias de despesa");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao buscar categorias de despesa" });
        }
    }

    /// <summary>
    /// Obtém apenas categorias de receita
    /// </summary>
    /// <returns>Lista de categorias de receita</returns>
    /// <response code="200">Retorna as categorias de receita</response>
    [HttpGet("incomes")]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetIncomeCategories()
    {
        try
        {
            _logger.LogInformation("Buscando categorias de receita");
            var categories = await _categoryService.GetIncomeCategoriesAsync();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar categorias de receita");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao buscar categorias de receita" });
        }
    }

    /// <summary>
    /// Cria uma nova categoria
    /// </summary>
    /// <param name="createDto">Dados para criação da categoria</param>
    /// <returns>Categoria criada</returns>
    /// <response code="201">Categoria criada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="409">Categoria com nome já existente</response>
    [HttpPost("create-category")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Dados inválidos para criação de categoria: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Criando nova categoria: {CategoryName}", createDto.Name);
            var category = await _categoryService.CreateAsync(createDto);

            _logger.LogInformation("Categoria criada com sucesso: {CategoryId}", category.Id);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }
        catch (DomainException ex) when (ex.Message.Contains("já existe"))
        {
            _logger.LogWarning(ex, "Tentativa de criar categoria com nome duplicado: {CategoryName}", createDto.Name);
            return Conflict(new { message = ex.Message });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para criação de categoria: {CategoryName}", createDto.Name);
            return BadRequest(ex.Message);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogWarning(ex, "DTO nulo para criação de categoria");
            return BadRequest(new { message = "Dados da categoria são obrigatórios" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar categoria: {CategoryName}", createDto.Name);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao criar a categoria" });
        }
    }

    /// <summary>
    /// Atualiza uma categoria existente
    /// </summary>
    /// <param name="id">ID da categoria</param>
    /// <param name="updateDto">Dados para atualização da categoria</param>
    /// <returns>Categoria atualizada</returns>
    /// <response code="200">Categoria atualizada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Categoria não encontrada</response>
    /// <response code="409">Nome já existe em outra categoria</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, [FromBody] UpdateCategoryDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Atualizando categoria: {CategoryId}", id);
            var category = await _categoryService.UpdateAsync(id, updateDto);

            return Ok(category);
        }
        catch (DomainException ex) when (ex.Message.Contains("não foi encontrada"))
        {
            _logger.LogWarning(ex, "Categoria não encontrada para atualização: {CategoryId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (DomainException ex) when (ex.Message.Contains("já existe"))
        {
            _logger.LogWarning(ex, "Nome de categoria já existe: {CategoryName}", updateDto.Name);
            return Conflict(new { message = ex.Message });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para atualização da categoria: {CategoryId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar categoria: {CategoryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao atualizar a categoria" });
        }
    }

    /// <summary>
    /// Verifica se uma categoria pode ser excluída
    /// </summary>
    /// <param name="id">ID da categoria</param>
    /// <returns>Resultado da verificação</returns>
    /// <response code="200">Retorna se a categoria pode ser excluída</response>
    /// <response code="404">Categoria não encontrada</response>
    [HttpGet("{id:guid}/can-delete")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> CanDelete(Guid id)
    {
        try
        {
            var canDelete = await _categoryService.CanDeleteAsync(id);

            if (!await _categoryService.ExistsAsync(id))
            {
                return NotFound(new { message = "Categoria não encontrada" });
            }

            return Ok(new
            {
                canDelete,
                message = canDelete
                    ? "Categoria pode ser excluída"
                    : "Categoria não pode ser excluída pois possui transações associadas"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar se categoria pode ser excluída: {CategoryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor." });
        }
    }

    /// <summary>
    /// Exclui uma categoria
    /// </summary>
    /// <param name="id">ID da categoria</param>
    /// <returns>Confirmação da exclusão</returns>
    /// <response code="204">Categoria excluída com sucesso</response>
    /// <response code="404">Categoria não encontrada</response>
    /// <response code="409">Categoria possui transações associadas</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            _logger.LogInformation("Excluindo categoria: {CategoryId}", id);
            await _categoryService.DeleteAsync(id);

            _logger.LogInformation("Categoria excluída com sucesso: {CategoryId}", id);
            return NoContent();
        }
        catch (DomainException ex) when (ex.Message.Contains("não foi encontrada"))
        {
            _logger.LogWarning(ex, "Categoria não encontrada para exclusão: {CategoryId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (DomainException ex) when (ex.Message.Contains("transações associadas"))
        {
            _logger.LogWarning(ex, "Tentativa de excluir categoria com transações: {CategoryId}", id);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir categoria: {CategoryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao excluir a categoria" });
        }
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetStats()
    {
        try
        {
            var totalCount = await _categoryService.GetTotalCountAsync();
            var expensesCategory = await _categoryService.GetExpenseCategoriesAsync();
            var incomesCategory = await _categoryService.GetIncomeCategoriesAsync();
            var categoriesWithTransactions = await _categoryService.GetCategoriesWithTransactionsAsync();

            var stats = new
            {
                totalCount,
                expensesCategoryCount = expensesCategory.Count(),
                incomesCategoryCount = incomesCategory.Count(),
                categoriesWithTransactionsCount = categoriesWithTransactions.Count()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas de categorias");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor ao obter estatísticas de categorias" });
        }
    }
}
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using FinanceTracker.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.API.Middlewares;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção não tratada capturada pelo middleware global: {ExceptionType} - {Message}",
                ex.GetType().Name, ex.Message);
            
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var errorResponse = CreateErrorResponse(exception);
        
        context.Response.StatusCode = errorResponse.StatusCode;
        
        LogException(exception, context, errorResponse);

        var jsonResponse = JsonSerializer.Serialize(errorResponse.Response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await context.Response.WriteAsync(jsonResponse);
    }

    private ErrorResponse CreateErrorResponse(Exception exception)
    {
        return exception switch
        {
            DomainException domainEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Response = new
                {
                    error = new
                    {
                        type = "DomainError",
                        message = domainEx.Message,
                        timestamp = DateTime.UtcNow,
                        traceId = Activity.Current?.Id
                    }
                }
            },
            ArgumentNullException argNullEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Response = new
                {
                    error = new
                    {
                        type = "ValidationError",
                        message = "Dados obrigatórios não foram fornecidos",
                        details = _env.IsDevelopment() ? argNullEx.Message : null,
                        timestamp = DateTime.UtcNow,
                        traceId = Activity.Current?.Id
                    }
                }
            },
            UnauthorizedAccessException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Response = new
                {
                    error = new
                    {
                        type = "AuthorizationError",
                        message = "Acesso não autorizado",
                        timestamp = DateTime.UtcNow,
                        traceId = Activity.Current?.Id
                    }
                }
            },
            NotImplementedException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.NotImplemented,
                Response = new
                {
                    error = new
                    {
                        type = "NotImplementedError",
                        message = "Funcionalidade não implementada",
                        timestamp = DateTime.UtcNow,
                        traceId = Activity.Current?.Id
                    }
                }
            },
            TimeoutException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.RequestTimeout,
                Response = new
                {
                    error = new
                    {
                        type = "TimeoutError",
                        message = "A operação excedeu o tempo limite",
                        timestamp = DateTime.UtcNow,
                        traceId = Activity.Current?.Id
                    }
                }
            },
            InvalidOperationException invalidOpEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Conflict,
                Response = new
                {
                    error = new
                    {
                        type = "ConflictError",
                        message = "Operação inválida no estado atual",
                        details = _env.IsDevelopment() ? invalidOpEx.Message : null,
                        timestamp = DateTime.UtcNow,
                        traceId = Activity.Current?.Id
                    }
                }
            },

            DbUpdateException dbUpdateEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Conflict,
                Response = new
                {
                    error = new
                    {
                        type = "DatabaseError",
                        message = "Erro ao atualizar dados no banco",
                        details = _env.IsDevelopment() ? GetDatabaseErrorDetails(dbUpdateEx) : null,
                        timestamp = DateTime.UtcNow,
                        traceId = Activity.Current?.Id
                    }
                }
            },
            HttpRequestException httpEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadGateway,
                Response = new
                {
                    error = new
                    {
                        type = "NetworkError",
                        message = "Erro de comunicação com serviços externos",
                        details = _env.IsDevelopment() ? httpEx.Message : null,
                        timestamp = DateTime.UtcNow,
                        traceId = Activity.Current?.Id
                    }
                }
            },
            _ => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Response = new
                {
                    error = new
                    {
                        type = "InternalServerError",
                        message = "Ocorreu um erro interno no servidor",
                        stackTrace = _env.IsDevelopment() ? exception.StackTrace : null,
                        details = _env.IsDevelopment() ? exception.ToString() : null,
                        timestamp = DateTime.UtcNow,
                        traceId = Activity.Current?.Id
                    }
                }
            }
        };
    }

    private void LogException(Exception exception, HttpContext context, ErrorResponse errorResponse)
    {
        var requestInfo = new
        {
            Method = context.Request.Method,
            Path = context.Request.Path,
            QueryString = context.Request.QueryString.Value,
            UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
            RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserId = context.User?.Identity?.Name,
            TraceId = Activity.Current?.Id
        };

        switch (errorResponse.StatusCode)
        {
            case >= 500:
                _logger.LogError(exception,
                    "Erro interno do servidor - Status: {StatusCode}, Request: {@RequestInfo}",
                    errorResponse.StatusCode, requestInfo);
                break;
            case >= 400 and < 500:
                _logger.LogWarning(exception,
                    "Erro do cliente - Status: {StatusCode}, Request: {@RequestInfo}",
                    errorResponse.StatusCode, requestInfo);
                break;
            
            default:
                _logger.LogInformation(exception,
                    "Exceção tratada - Status: {StatusCode}, Request: {@RequestInfo}",
                    errorResponse.StatusCode, requestInfo);
                break;
        }

        if (exception is DomainException)
        {
            _logger.LogWarning("Regra de negócio violada: {DomainError} - Path: {RequestPath}",
                exception.Message, context.Request.Path);
        } else if (exception is DbUpdateException)
        {
            _logger.LogError("Erro de banco de dados: {DatabaseError} - Path: {RequestPath}",
                GetDatabaseErrorDetails(exception), context.Request.Path);
        }
    }

    private string GetDatabaseErrorDetails(Exception dbException)
    {
        if (dbException.InnerException?.Message.Contains("duplicate key") == true)
        {
            return "Tentativa de inserir dados duplicados";
        }

        if (dbException.InnerException?.Message.Contains("foreign key") == true)
        {
            return "Violação de chave estrangeira - registro referenciado não existe";
        }
        
        if (dbException.InnerException?.Message.Contains("timeout") == true)
        {
            return "Timeout de conexão com o banco de dados";
        }
        
        return dbException.InnerException?.Message ?? dbException.Message;

    }

    private record ErrorResponse
    {
        public int StatusCode { get; init; }
        public object Response { get; init; } = null!;
    }
}
namespace FinanceTracker.API.Middlewares;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        context.Items["CorrelationId"] = correlationId;
        
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   ["CorrelationId"] = correlationId,
                   ["RequestPath"] = context.Request.Path.Value ?? string.Empty,
                   ["RequestMethod"] = context.Request.Method
               }))
        {
            await _next(context);
        }
    }
    private string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId))
        {
            var headerValue = correlationId.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(headerValue))
            {
                return headerValue;
            }
        }

        if (!string.IsNullOrWhiteSpace(context.TraceIdentifier))
        {
            return context.TraceIdentifier;
        }

        return Guid.NewGuid().ToString("D");
    }
}


/// <summary>
/// Extension para acessar Correlation ID em qualquer lugar da aplicação
/// </summary>
public static class HttpContextExtensions
{
    public static string GetCorrelationId(this HttpContext context)
    {
        return context.Items["CorrelationId"]?.ToString() ?? context.TraceIdentifier;  
    }
}
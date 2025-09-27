using System.Diagnostics;
using System.Text;

namespace FinanceTracker.API.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var requestInfo = await CaptureRequestInfoAsync(context);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var responseInfo = new
            {
                StatusCode = context.Response.StatusCode,
                ContentType = context.Response.ContentType,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
            LogRequest(requestInfo, responseInfo);
        }
    }

    private async Task<object> CaptureRequestInfoAsync(HttpContext context)
    {
        var request = context.Request;

        string? body = null;
        if(ShouldLogBody(request) && request.ContentLength.HasValue && request.ContentLength < 10000)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        return new
        {
            Method = request.Method,
            Path = request.Path.Value,
            QueryString = request.QueryString.Value,
            ContentType = request.ContentType,
            ContentLength = request.ContentLength,
            UserAgent = request.Headers["User-Agent"].FirstOrDefault(),
            RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserId = context.User?.Identity?.Name,
            CorrelationId = context.TraceIdentifier,
            Body = ShouldLogBody(request) ? body : "[Body não logado]",
            Headers = GetSafeHeaders(request.Headers)
        };
    }

    private void LogRequest(object requestInfo, object responseInfo)
    {
        var statusCode = ((dynamic)requestInfo).StatusCode;
        if (statusCode >= 500)
        {
            _logger.LogError("Request processada com erro do servidor: {@RequestInfo} -> {@ResponseInfo}",
                requestInfo, responseInfo);
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning("Request processada com erro do cliente: {@RequestInfo} -> {@ResponseInfo}",
                requestInfo, responseInfo);
        }
        else if(((dynamic)responseInfo).ElapsedMs > 5000)
        {
            _logger.LogInformation("Request completed - Request: {@RequestInfo}, Response: {@ResponseInfo}", requestInfo, responseInfo);
        }
        else
        {
            LoggerExtensions.LogInformation(_logger, "Request processada: {Method} {Path} -> {StatusCode} ({ElapsedMs}ms)",
                ((dynamic)requestInfo).Method,
                ((dynamic)requestInfo).Path,
                statusCode,
                ((dynamic)responseInfo).ElapsedMs);
        }
    }

    private static bool ShouldLogBody(HttpRequest request)
    {
        if (!HttpMethods.IsPost(request.Method) &&
            !HttpMethods.IsPut(request.Method) &&
            !HttpMethods.IsPatch(request.Method))
        {
            return false;
        }
        
        if (request.ContentType?.StartsWith("multipart/") == true ||
            request.ContentType?.StartsWith("application/octet-stream") == true)
        {
            return false;
        }

        return true;
    }

    private static Dictionary<string, string> GetSafeHeaders(IHeaderDictionary headers)
    {
        var sensitiveHeaders = new[] {"authorization", "cookie", "x-api-key", "x-auth-token"};
        
        return headers.
            Where(h => !sensitiveHeaders.Contains(h.Key.ToLower())).
            ToDictionary(h => h.Key, h => h.Value.ToString());
    }
}
namespace FinanceTracker.API.Middlewares;

public static class MiddlewareExtensions
{
    /// <summary>
    /// Adiciona o middleware de tratamento global de exceções
    /// </summary>
    /// <param name="app">Aplicação</param>
    /// <returns>Aplicação configurada</returns>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }

    /// <summary>
    /// Adiciona middleware de logging de requisições
    /// </summary>
    /// <param name="app">Aplicação</param>
    /// <returns>Aplicação configurada</returns>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
    
    /// <summary>
    /// Adiciona middleware de correlação de requisições
    /// </summary>
    /// <param name="app">Aplicação</param>
    /// <returns>Aplicação configurada</returns>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
    
    /// <summary>
    /// Adiciona todos os middlewares customizados na ordem correta
    /// </summary>
    /// <param name="app">Aplicação</param>
    /// <returns>Aplicação configurada</returns>
    public static IApplicationBuilder UseCustomMiddlewares(this IApplicationBuilder app)
    {
        return app.UseCorrelationId()
                  .UseRequestLogging()
                  .UseGlobalExceptionHandling();
    }
    
    
}
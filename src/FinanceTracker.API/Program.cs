using FinanceTracker.API.Middlewares;
using FinanceTracker.Infrastructure.Data;
using FinanceTracker.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore;
using static FinanceTracker.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

ValidateConfiguration(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Finance Tracker API",
        Version = "v1",
        Description = "API para gerenciamento de transações financeiras"
    });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowCredentials()
            .AllowAnyMethod()
            .WithExposedHeaders("X-Correlation-ID");
    });
});

builder.Services.AddRouting(options => options.LowercaseUrls = true);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddInfrastructureForDevelopment(builder.Configuration);
}
else
{
    builder.Services.AddInfrastructureForProduction(builder.Configuration);
}

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    if (builder.Environment.IsDevelopment())
    {
        logging.AddDebug();
    }
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Finance Tracker API V1");
    c.RoutePrefix = string.Empty;
});

if (app.Environment.IsDevelopment())
{
    try
    {
        await ApplyMigrationsAsync(app.Services);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro na inicialização do banco de dados.");
        
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

app.UseCustomMiddlewares();

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.MapGet("/info", (HttpContext context) => new
{
    Application = "Finance Tracker API",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow,
    CorrelationId = context.GetCorrelationId()
});

app.Run();




using FinanceTracker.Application.DTOs.Category;
using FinanceTracker.Application.DTOs.Transaction;
using FinanceTracker.Application.Services.Implementations;
using FinanceTracker.Application.Services.Interfaces;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Infrastructure.Data;
using FinanceTracker.Infrastructure.Repositories;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
    
    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DevConnection") ??
                               throw new InvalidOperationException("Connection string 'Dev' não foi encontrada.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);

                npgsqlOptions.CommandTimeout(30);

                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name);
            });
            
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
            
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        });

        services
            .AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("PostgreSQL Database",
                failureStatus: HealthStatus.Unhealthy);
    }
    
    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
    }
    
    private static void AddApplicationServices(IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IDashboardService, DashboardService>();
    }

    private static void AddMappings(IServiceCollection services)
    {
        var config = new TypeAdapterConfig();

        config.Default.PreserveReference(true);
        config.Default.IgnoreNullValues(true);

        ConfigureCustomMappings(config);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
    }
    
    private static void ConfigureCustomMappings(TypeAdapterConfig config)
    {
        config.NewConfig<Category, CategoryDto>()
            .Map(dest => dest.DisplayName, src => src.DisplayName)
            .Map(dest => dest.IsExpenseCategory, src => src.IsExpenseCategory)
            .Map(dest => dest.IsIncomeCategory, src => src.IsIncomeCategory)
            .Map(dest => dest.TransactionType, src => src.TransactionType);

        config.NewConfig<Category, CategorySummaryDto>()
            .Map(dest => dest.DisplayName, src => src.DisplayName)
            .Map(dest => dest.TransactionType, src => src.TransactionType);

        config.NewConfig<Transaction, TransactionDto>()
            .Map(dest => dest.Amount, src => src.Amount.Amount)
            .Map(dest => dest.FormattedAmount, src => src.Amount.ToString())
            .Map(dest => dest.FormattedDate, src => src.TransactionDate.ToString("dd/MM/yyyy"))
            .Map(dest => dest.IsIncome, src => src.IsIncome)
            .Map(dest => dest.IsExpense, src => src.IsExpense)
            .Map(dest => dest.TransactionType, src => src.TransactionType);

        config.NewConfig<Transaction, TransactionSummaryDto>()
            .Map(dest => dest.Amount, src => src.Amount.Amount)
            .Map(dest => dest.FormattedAmount, src => src.Amount.ToString())
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : "Sem categoria");
    }

    public static void AddValidations(IServiceCollection services)
    {
        // FluentValidation (quando implementar)
        // services.AddValidatorsFromAssemblyContaining<CreateTransactionValidator>();
        // services.AddFluentValidationAutoValidation();
        // services.AddFluentValidationClientsideAdapters();
    }

    public static async Task ApplyMigrationsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            await dbContext.Database.MigrateAsync();
        } catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetService<ILogger<ApplicationDbContext>>();
            logger?.LogError(ex, "Erro ao aplicar migrations do banco de dados");
            throw;
        }
    }
    
    public static void ValidateConfiguration(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DevConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' é obrigatória.");
        }

        // Validar outras configurações necessárias
        // var jwtKey = configuration["JwtSettings:Key"];
        // if (string.IsNullOrEmpty(jwtKey)) { ... }
    }
}
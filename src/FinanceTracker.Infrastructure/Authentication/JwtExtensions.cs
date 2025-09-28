using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace FinanceTracker.Infrastructure.Authentication;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey é obrigatório");
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JwtSettings:Issuer é obrigatório");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JwtSettings:Audience é obrigatório");
        
        if (secretKey.Length < 32)
            throw new InvalidOperationException("JwtSettings:SecretKey deve ter pelo menos 32 caracteres");
        
        var key = Encoding.UTF8.GetBytes(secretKey);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    RequireExpirationTime = true,
                    ValidateActor = false,
                    ValidateTokenReplay = false
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogWarning("Falha na autenticação JWT: {Error}", context.Exception.Message);

                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers["Token-Expired"] = "true";
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        var userId = context.Principal?.FindFirst("userId")?.Value;
                        var email = context.Principal?.FindFirst("email")?.Value;

                        logger.LogDebug("Token JWT validado para usuário {UserId} ({Email})", userId, email);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogWarning("Challenge JWT acionado: {Error}, {ErrorDescription}",
                            context.Error, context.ErrorDescription);
                        return Task.CompletedTask;
                    }
                };
            });
        
        return services;
    }

    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            
            options.AddPolicy("ActiveUser", policy => 
                policy.RequireAuthenticatedUser()
                    .RequireClaim("isActive", "true"));
            
            options.AddPolicy("EmailConfirmed", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("emailConfirmed", "true"));
            
            options.AddPolicy("FullyVerified", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("isActive", "true")
                    .RequireClaim("emailConfirmed", "true"));
            
            options.AddPolicy("Admin", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("role", "Admin"));
            
        });

        return services;
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FinanceTracker.Application.Services.Interfaces;
using FinanceTracker.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace FinanceTracker.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpirationMinutes;
    private readonly int _refreshTokenExpirationDays;

    public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _secretKey = _configuration["JwtSettings:SecretKey"] 
                     ?? throw new InvalidOperationException("JwtSettings:SecretKey não configurado");
        _issuer = _configuration["JwtSettings:Issuer"] 
                  ?? throw new InvalidOperationException("JwtSettings:Issuer não configurado");
        _audience = _configuration["JwtSettings:Audience"] 
                    ?? throw new InvalidOperationException("JwtSettings:Audience não configurado");
        
        if (!int.TryParse(_configuration["JwtSettings:AccessTokenExpirationMinutes"], out _accessTokenExpirationMinutes))
            _accessTokenExpirationMinutes = 60; // Padrão: 1 hora

        if (!int.TryParse(_configuration["JwtSettings:RefreshTokenExpirationDays"], out _refreshTokenExpirationDays))
            _refreshTokenExpirationDays = 7; // Padrão: 7 dias
        
        if (_secretKey.Length < 32)
            throw new InvalidOperationException("JwtSettings:SecretKey deve ter pelo menos 32 caracteres");
    }


    public string GenerateAccessToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new("userId", user.Id.ToString()),
            new("email", user.Email),
            new("name", user.Name),
            new("emailConfirmed", user.EmailConfirmed.ToString().ToLowerInvariant()),
            new("isActive", user.IsActive.ToString().ToLowerInvariant()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        
        _logger.LogInformation("Access token gerado para usuário {UserId} ({Email})", user.Id, user.Email);
        return tokenString;
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        var refreshToken = Convert.ToBase64String(randomBytes);
        _logger.LogDebug("Refresh token gerado");
        return refreshToken;
    }

    public string GenerateEmailConfirmationToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new("userId", user.Id.ToString()),
            new("email", user.Email),
            new("purpose", "email_confirmation"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24), // Token válido por 24 horas
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GeneratePasswordResetToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new("email", user.Email),
            new("userId", user.Id.ToString()),
            new("purpose", "password_reset"),
            new("passwordHash", user.PasswordHash[..10]), // Primeiros 10 chars como validação
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2), // Token válido por 2 horas
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public bool ValidateToken(string token, out ClaimsPrincipal? principal)
    {
        principal = null;

        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1) // Tolerância de 1 minuto
            };

            principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha na validação do token");
            return false;
        }
    }

    public bool ValidateEmailConfirmationToken(string token, Guid userId)
    {
        if (!ValidateToken(token, out var principal) || principal == null)
            return false;

        var purposeClaim = principal.FindFirst("purpose")?.Value;
        var userIdClaim = principal.FindFirst("userId")?.Value;

        return purposeClaim == "email_confirmation" && 
               userIdClaim == userId.ToString();
    }

    public bool ValidatePasswordResetToken(string token, string email)
    {
        if (!ValidateToken(token, out var principal) || principal == null)
            return false;

        var purposeClaim = principal.FindFirst("purpose")?.Value;
        var emailClaim = principal.FindFirst("email")?.Value;

        return purposeClaim == "password_reset" && 
               emailClaim?.Equals(email, StringComparison.OrdinalIgnoreCase) == true;
    }

    public DateTime GetTokenExpiration(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return DateTime.MinValue;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            return jwtToken.ValidTo;
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    public Guid GetUserIdFromToken(string token)
    {
        if (!ValidateToken(token, out var principal) || principal == null)
            return Guid.Empty;

        var userIdClaim = principal.FindFirst("userId")?.Value;
        
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    public string GetEmailFromToken(string token)
    {
        if (!ValidateToken(token, out var principal) || principal == null)
            return string.Empty;

        return principal.FindFirst("email")?.Value ?? string.Empty;
    }
}
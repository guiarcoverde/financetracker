using System.Security.Claims;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string GenerateEmailConfirmationToken();
    string GeneratePasswordResetToken();
    
    bool ValidateToken(string token, out ClaimsPrincipal? principal);
    bool ValidateEmailConfirmationToken(string token, Guid userId);
    bool ValidatePasswordResetToken(string token, string email);
    
    DateTime GetTokenExpiration(string token);
    Guid GetUserIdFromToken(string token);
    string GetEmailFromToken(string token);
}
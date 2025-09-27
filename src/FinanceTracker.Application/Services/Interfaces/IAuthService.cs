using FinanceTracker.Application.DTOs.Auth;
using FinanceTracker.Application.DTOs.User;

namespace FinanceTracker.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginDto loginDto);
    Task<AuthResultDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task LogoutAsync(Guid userId);
    Task LogoutAllAsync(Guid userId);
    
    Task<AuthResultDto> RegisterAsync(RegisterDto registerDto);
    Task<bool> ConfirmEmailAsync(Guid userId, string confirmationToken);
    
    Task RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);
    
    Task<UserProfileDto> GetProfileAsync(Guid userId);
    Task UpdateProfileAsync(Guid userId, UpdateProfileDto updateProfileDto);
}
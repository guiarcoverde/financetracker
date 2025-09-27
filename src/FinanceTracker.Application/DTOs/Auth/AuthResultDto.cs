using FinanceTracker.Application.DTOs.User;

namespace FinanceTracker.Application.DTOs.Auth;

public class AuthResultDto
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserProfileDto? User { get; set; }
    public string? Message { get; set; }
}
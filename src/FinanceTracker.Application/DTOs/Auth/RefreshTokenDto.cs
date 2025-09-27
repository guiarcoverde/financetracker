using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Application.DTOs.Auth;

public class RefreshTokenDto
{
    [Required(ErrorMessage = "Access token é obrigatório")]
    public string AccessToken { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Refresh token é obrigatório")]
    public string RefreshToken { get; set; } = string.Empty;
}
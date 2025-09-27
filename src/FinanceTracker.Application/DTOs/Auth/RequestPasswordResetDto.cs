using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Application.DTOs.Auth;

public class RequestPasswordResetDto
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string Email { get; set; } = string.Empty;
}
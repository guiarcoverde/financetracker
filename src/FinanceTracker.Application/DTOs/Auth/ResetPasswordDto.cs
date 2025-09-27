using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Application.DTOs.Auth;

public class ResetPasswordDto
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Token é obrigatório")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", 
        ErrorMessage = "Senha deve conter pelo menos uma letra maiúscula, uma minúscula e um número")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação da nova senha é obrigatória")]
    [Compare("NewPassword", ErrorMessage = "Senhas não coincidem")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
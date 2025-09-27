using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Application.DTOs.Auth;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Senha atual é obrigatória")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", 
        ErrorMessage = "Senha deve conter pelo menos uma letra maiúscula, uma minúscula e um número")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação da nova senha é obrigatória")]
    [Compare("NewPassword", ErrorMessage = "Senhas não coincidem")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
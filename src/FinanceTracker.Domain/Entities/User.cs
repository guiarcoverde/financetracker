using System.Security.Cryptography;
using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string Name { get; private set; }
    public string PasswordHash { get; private set; } 
    public string Salt { get; private set; } 
    public bool IsActive { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime CreatedAt { get; private set; } 
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }
    
    private User(){ }

    public User(string email, string name, string password)
    {
        ValidateEmail(email);
        ValidateName(name);
        ValidatePassword(password);
        
        Id = Guid.NewGuid();
        Email = email.ToLowerInvariant().Trim();
        Name = name.Trim();
        Salt = GenerateSalt();
        PasswordHash = HashPassword(password, Salt);
        IsActive = true;
        EmailConfirmed = true;
        CreatedAt = DateTime.UtcNow;
    }
    
    public bool VerifyPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;
        
        var hashedPassword = HashPassword(password, Salt);
        return hashedPassword == PasswordHash;
    }
    
    public void ChangePassword(string currentPassword, string newPassword)
    {
        if (!VerifyPassword(currentPassword))
            throw new ArgumentException("Senha atual incorreta");
        
        ValidatePassword(newPassword);
        
        Salt = GenerateSalt();
        PasswordHash = HashPassword(newPassword, Salt);
    }

    public void UpdateName(string newName)
    {
        ValidateName(newName);
        Name = newName.Trim();
    }
    
    public void UpdateEmail(string newEmail)
    {
        ValidateEmail(newEmail);
        Email = newEmail.ToLowerInvariant().Trim();
        EmailConfirmed = true;
    }
    
    public void ConfirmEmail()
    {
        EmailConfirmed = true;
    }

    public void Activate()
    {
        IsActive = true;
    }
    
    public void Deactivate()
    {
        IsActive = false;
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
    }
    
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void SetRefreshToken(string refreshToken, DateTime expiryTime)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new DomainException("Refresh token não pode ser vazio");

        if (expiryTime <= DateTime.UtcNow)
            throw new DomainException("Refresh token deve ter data de expiração futura");
        
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
    }
    
    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
    }
    
    public bool IsRefreshTokenValid(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken) || string.IsNullOrWhiteSpace(RefreshToken))
            return false;
        
        if (RefreshTokenExpiryTime == null || RefreshTokenExpiryTime <= DateTime.UtcNow)
            return false;
        
        return RefreshToken == refreshToken;
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email é obrigatório");

        email = email.Trim();
        
        if(email.Length > 200)
            throw new DomainException("Email não pode exceder 200 caracteres");

        if (!email.Contains('@') || !email.Contains('.') || email.IndexOf('.') > email.LastIndexOf('.'))
            throw new DomainException("Formato de email inválido");

        if (email.StartsWith('@') || email.EndsWith('@') || email.Contains(".."))
            throw new DomainException("Formato de email inválido");
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome é obrigatório");

        name = name.Trim();

        if (name.Length < 2)
            throw new DomainException("Nome deve ter pelo menos 2 caracteres");

        if (name.Length > 100)
            throw new DomainException("Nome não pode exceder 100 caracteres");
    }
    
    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new DomainException("Senha é obrigatória");

        if (password.Length < 6)
            throw new DomainException("Senha deve ter pelo menos 6 caracteres");

        if (password.Length > 100)
            throw new DomainException("Senha não pode exceder 100 caracteres");
        
        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);

        if (!hasUpper || !hasLower || !hasDigit)
            throw new DomainException("Senha deve conter pelo menos uma letra maiúscula, uma minúscula e um número");
    }

    private static string GenerateSalt()
    {
        byte[] saltBytes = new byte[32];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        
        return Convert.ToBase64String(saltBytes);
    }

    private static string HashPassword(string password, string salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(32);
        return Convert.ToBase64String(hash);
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not User other)
            return false;

        return Id == other.Id;
    }
    
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
    
    public override string ToString()
    {
        return $"User: {Name} ({Email})";
    }
}
using ModelHub.Domain.Authorization;
using ModelHub.Domain.Exceptions;

namespace ModelHub.Domain.Entities;

public sealed class AppUser : BaseEntity
{
    private AppUser()
    {
    }

    public AppUser(string email, string role = "Analyst")
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BusinessRuleException("Email is required.");
        }

        Email = email.Trim().ToLowerInvariant();
        Role = role;
        IsActive = true;
    }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    //public string Role { get; private set; } = "Analyst";
    public string Role { get; set; } = AppRoles.Analyst;

    public bool IsActive { get; private set; }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new BusinessRuleException("Password hash is required.");
        }

        PasswordHash = passwordHash;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}

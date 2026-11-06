using System;
using System.ComponentModel.DataAnnotations;
using ModelHub.Domain.Authorization;
using ModelHub.Domain.Exceptions;

namespace ModelHub.Domain.Entities;

public sealed class AppUser : BaseEntity
{
    private AppUser()
    {
        // Required by Entity Framework Core.
    }

    public AppUser(
        string email,
        string role = AppRoles.Analyst)
    {
        ChangeEmail(email);
        ChangeRole(role);

        IsActive = true;
    }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string Role { get; private set; } = AppRoles.Analyst;

    public bool IsActive { get; private set; }

    public void ChangeEmail(string email)
    {
        var normalizedEmail = email?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalizedEmail)
            || normalizedEmail.Length > 320
            || !new EmailAddressAttribute().IsValid(normalizedEmail))
        {
            throw new BusinessRuleException(
                "A valid email of at most 320 characters is required.");
        }

        Email = normalizedEmail;
    }

    public void ChangeRole(string role)
    {
        if (!AppRoles.IsValid(role))
        {
            throw new BusinessRuleException(
                "Role must be Admin, Analyst, or Reviewer.");
        }

        Role = role;
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new BusinessRuleException(
                "Password hash is required.");
        }

        PasswordHash = passwordHash;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}

using Microsoft.AspNetCore.Identity;
using ModelHub.Application.Interfaces;
using ModelHub.Domain.Entities;

namespace ModelHub.Infrastructure.Authentication;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public string HashPassword(AppUser user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(AppUser user, string passwordHash, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, passwordHash, password);

        return result != PasswordVerificationResult.Failed;
    }
}

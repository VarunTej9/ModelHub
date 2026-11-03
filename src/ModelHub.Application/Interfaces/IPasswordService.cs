using ModelHub.Domain.Entities;

namespace ModelHub.Application.Interfaces;

public interface IPasswordService
{
    string HashPassword(AppUser user, string password);

    bool VerifyPassword(AppUser user, string passwordHash, string password);
}

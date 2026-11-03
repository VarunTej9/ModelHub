using ModelHub.Domain.Entities;

namespace ModelHub.Application.Interfaces;

public interface IJwtTokenService
{
    string CreateToken(AppUser user);
}

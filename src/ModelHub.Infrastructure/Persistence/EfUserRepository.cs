using Microsoft.EntityFrameworkCore;
using ModelHub.Application.Interfaces;
using ModelHub.Domain.Entities;

namespace ModelHub.Infrastructure.Persistence;

public sealed class EfUserRepository : IUserRepository
{
    private readonly ModelHubDbContext _context;

    public EfUserRepository(ModelHubDbContext context)
    {
        _context = context;
    }

    public Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _context.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public async Task AddAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }
}

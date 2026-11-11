using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ModelHub.Application.Exceptions;
using ModelHub.Application.Interfaces;
using ModelHub.Domain.Entities;

namespace ModelHub.Infrastructure.Persistence;

public sealed class UserManagementRepository : IUserManagementRepository
{
    private readonly ModelHubDbContext _db;

    public UserManagementRepository(ModelHubDbContext db)
    {
        _db = db;
    }

    public Task<AppUser?> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        return _db.Users.SingleOrDefaultAsync(
            user => user.Id == userId,
            cancellationToken);
    }

    public Task<bool> EmailExistsAsync(
        string email,
        int? excludedUserId,
        CancellationToken cancellationToken)
    {
        var query = _db.Users.Where(
            user => user.Email == email);

        if (excludedUserId.HasValue)
        {
            var id = excludedUserId.Value;

            query = query.Where(
                user => user.Id != id);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        bool? isActive,
        CancellationToken cancellationToken)
    {
        return Filter(isActive)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AppUser>> ListAsync(
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return await Filter(isActive)
            .OrderBy(user => user.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        AppUser user,
        CancellationToken cancellationToken)
    {
        await _db.Users.AddAsync(
            user,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await _db.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqliteException
            {
                SqliteExtendedErrorCode: 2067
            } sqliteException
            && sqliteException.Message.Contains(
                "Users.Email",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new UserManagementException(
                UserManagementError.DuplicateEmail,
                "An account with this email already exists.",
                exception);
        }
    }

    private IQueryable<AppUser> Filter(bool? isActive)
    {
        var query = _db.Users.AsNoTracking();

        if (isActive.HasValue)
        {
            var active = isActive.Value;

            query = query.Where(
                user => user.IsActive == active);
        }

        return query;
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ModelHub.Domain.Entities;

namespace ModelHub.Application.Interfaces;

public interface IUserManagementRepository
{
    Task<AppUser?> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(
        string email,
        int? excludedUserId,
        CancellationToken cancellationToken);

    Task<int> CountAsync(
        bool? isActive,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AppUser>> ListAsync(
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task AddAsync(
        AppUser user,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}

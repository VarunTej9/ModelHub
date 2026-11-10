using System.Threading;
using System.Threading.Tasks;
using ModelHub.Application.Contracts;

namespace ModelHub.Application.Interfaces;

public interface IUserManagementService
{
    Task<UserResponse> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedUsersResponse> ListAsync(
        bool? isActive = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<UserResponse> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<UserResponse> UpdateAsync(
        int userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<UserResponse> ChangeRoleAsync(
        int userId,
        ChangeUserRoleRequest request,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        int userId,
        CancellationToken cancellationToken = default);
}

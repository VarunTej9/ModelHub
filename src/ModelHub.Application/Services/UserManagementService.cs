using ModelHub.Application.Contracts;
using ModelHub.Application.Exceptions;
using ModelHub.Application.Interfaces;
using ModelHub.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace ModelHub.Application.Services;

public sealed class UserManagementService : IUserManagementService
{
    private readonly IUserManagementRepository _users;
    private readonly IPasswordService _passwords;

    public UserManagementService(
        IUserManagementRepository users,
        IPasswordService passwords)
    {
        _users = users;
        _passwords = passwords;
    }

    public async Task<UserResponse> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);

        var email = NormalizeEmail(request.Email);

        await EnsureEmailAvailableAsync(
            email,
            excludedUserId: null,
            cancellationToken: cancellationToken);

        var user = new AppUser(
            email,
            request.Role);

        var passwordHash = _passwords.HashPassword(
            user,
            request.Password);

        user.SetPasswordHash(passwordHash);

        await _users.AddAsync(
            user,
            cancellationToken);

        await _users.SaveChangesAsync(
            cancellationToken);

        return Map(user);
    }

    public async Task<PagedUsersResponse> ListAsync(
        bool? isActive = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            throw new UserManagementException(
                UserManagementError.Validation,
                "Page must be positive and pageSize must be between 1 and 100.");
        }

        var offset = (long)(page - 1) * pageSize;

        if (offset > int.MaxValue)
        {
            throw new UserManagementException(
                UserManagementError.Validation,
                "The requested page is too large.");
        }

        var totalCount = await _users.CountAsync(
            isActive,
            cancellationToken);

        var users = await _users.ListAsync(
            isActive,
            page,
            pageSize,
            cancellationToken);

        var items = users
            .Select(Map)
            .ToArray();

        return new PagedUsersResponse(
            page,
            pageSize,
            totalCount,
            items);
    }

    public async Task<UserResponse> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await FindAsync(
            userId,
            cancellationToken);

        return Map(user);
    }

    public async Task<UserResponse> UpdateAsync(
        int userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);

        var user = await FindAsync(
            userId,
            cancellationToken);

        var email = NormalizeEmail(request.Email);

        await EnsureEmailAvailableAsync(
            email,
            excludedUserId: userId,
            cancellationToken: cancellationToken);

        user.ChangeEmail(email);

        await _users.SaveChangesAsync(
            cancellationToken);

        return Map(user);
    }

    public async Task<UserResponse> ChangeRoleAsync(
        int userId,
        ChangeUserRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);

        var user = await FindAsync(
            userId,
            cancellationToken);

        user.ChangeRole(request.Role);

        await _users.SaveChangesAsync(
            cancellationToken);

        return Map(user);
    }

    public async Task DeactivateAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await FindAsync(
            userId,
            cancellationToken);

        // Repeated deactivation is safe.
        if (!user.IsActive)
        {
            return;
        }

        user.Deactivate();

        await _users.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<AppUser> FindAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        if (userId < 1)
        {
            throw new UserManagementException(
                UserManagementError.Validation,
                "User ID must be positive.");
        }

        var user = await _users.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            throw new UserManagementException(
                UserManagementError.NotFound,
                "User not found.");
        }

        return user;
    }

    private async Task EnsureEmailAvailableAsync(
        string email,
        int? excludedUserId,
        CancellationToken cancellationToken)
    {
        var exists = await _users.EmailExistsAsync(
            email,
            excludedUserId,
            cancellationToken);

        if (exists)
        {
            throw new UserManagementException(
                UserManagementError.DuplicateEmail,
                "An account with this email already exists.");
        }
    }

    private static void Validate(object request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            errors,
            validateAllProperties: true);

        if (!valid)
        {
            var message = string.Join(
                " ",
                errors.Select(error => error.ErrorMessage));

            throw new UserManagementException(
                UserManagementError.Validation,
                message);
        }
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static UserResponse Map(AppUser user)
    {
        return new UserResponse(
            user.Id,
            user.Email,
            user.Role,
            user.IsActive,
            user.CreatedUtc,
            user.UpdatedUtc);
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ModelHub.Application.Contracts;

public sealed class CreateUserRequest
{
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 12)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [RegularExpression(
        "^(Admin|Analyst|Reviewer)$",
        ErrorMessage = "Role must be Admin, Analyst, or Reviewer.")]
    public string Role { get; init; } = string.Empty;
}

public sealed class UpdateUserRequest
{
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; init; } = string.Empty;
}

public sealed class ChangeUserRoleRequest
{
    [Required]
    [RegularExpression(
        "^(Admin|Analyst|Reviewer)$",
        ErrorMessage = "Role must be Admin, Analyst, or Reviewer.")]
    public string Role { get; init; } = string.Empty;
}

public sealed record UserResponse(
    int Id,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedUtc,
    DateTime? UpdatedUtc);

public sealed record PagedUsersResponse(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<UserResponse> Items);

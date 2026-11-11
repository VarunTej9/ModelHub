using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelHub.Application.Contracts;
using ModelHub.Application.Interfaces;

namespace ModelHub.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminController : ControllerBase
{
    private readonly IUserManagementService _users;

    public AdminController(IUserManagementService users)
    {
        _users = users;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(UserResponse),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<UserResponse>> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _users.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { userId = user.Id },
            user);
    }

    [HttpGet]
    public async Task<ActionResult<PagedUsersResponse>> List(
        CancellationToken cancellationToken,
        [FromQuery] bool? isActive = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20)
    {
        var users = await _users.ListAsync(
            isActive,
            page,
            pageSize,
            cancellationToken);

        return Ok(users);
    }

    [HttpGet("{userId:int}")]
    public async Task<ActionResult<UserResponse>> GetById(
        int userId,
        CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(
            userId,
            cancellationToken);

        return Ok(user);
    }

    [HttpPut("{userId:int}")]
    public async Task<ActionResult<UserResponse>> Update(
        int userId,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _users.UpdateAsync(
            userId,
            request,
            cancellationToken);

        return Ok(user);
    }

    [HttpPut("{userId:int}/role")]
    public async Task<ActionResult<UserResponse>> ChangeRole(
        int userId,
        [FromBody] ChangeUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _users.ChangeRoleAsync(
            userId,
            request,
            cancellationToken);

        return Ok(user);
    }

    [HttpPost("{userId:int}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate(
        int userId,
        CancellationToken cancellationToken)
    {
        await _users.DeactivateAsync(
            userId,
            cancellationToken);

        return NoContent();
    }
}

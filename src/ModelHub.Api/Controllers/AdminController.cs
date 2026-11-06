using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelHub.Domain.Authorization;
using ModelHub.Infrastructure.Persistence;

namespace ModelHub.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminController(ModelHubDbContext dbContext)
    : ControllerBase
{
    [HttpPut("users/{userId:int}/role")]
    public async Task<IActionResult> AssignRole(
        int userId,
        AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        if (!AppRoles.IsValid(request.Role))
        {
            return BadRequest(new
            {
                message = "Role must be Admin, Analyst, or Reviewer."
            });
        }

        var user = await dbContext.Users
            .SingleOrDefaultAsync(
                user => user.Id == userId,
                cancellationToken);

        if (user is null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        user.ChangeRole(AppRoles.Reviewer);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

public sealed record AssignRoleRequest(string Role);

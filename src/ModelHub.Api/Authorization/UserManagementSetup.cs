using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelHub.Api.Middleware;
using ModelHub.Application.Interfaces;
using ModelHub.Application.Services;
using ModelHub.Infrastructure.Persistence;

namespace ModelHub.Api.Authorization;

public static class UserManagementSetup
{
    public static IServiceCollection AddUserManagement(
        this IServiceCollection services)
    {
        services.AddScoped<
            IUserManagementRepository,
            UserManagementRepository>();

        services.AddScoped<
            IUserManagementService,
            UserManagementService>();

        // Extend your existing Bearer configuration.
        // Do not register JWT authentication a second time.
        services.PostConfigure<JwtBearerOptions>(
            JwtBearerDefaults.AuthenticationScheme,
            options =>
            {
                var previousHandler =
                    options.Events.OnTokenValidated;

                options.Events.OnTokenValidated = async context =>
                {
                    await previousHandler(context);

                    if (context.Result is not null
                        && !context.Result.Succeeded)
                    {
                        return;
                    }

                    await ValidateCurrentUserAsync(context);
                };
            });

        return services;
    }

    public static IApplicationBuilder UseUserManagementErrors(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<
            UserManagementExceptionMiddleware>();
    }

    private static async Task ValidateCurrentUserAsync(
        TokenValidatedContext context)
    {
        var userIdClaim = context.Principal?
            .FindFirst(ClaimTypes.NameIdentifier)?
            .Value;

        var roleClaim = context.Principal?
            .FindFirst(ClaimTypes.Role)?
            .Value;

        if (!int.TryParse(userIdClaim, out var userId)
            || userId < 1)
        {
            context.Fail("Invalid user identity.");
            return;
        }

        var db = context.HttpContext
            .RequestServices
            .GetRequiredService<ModelHubDbContext>();

        var user = await db.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new
            {
                user.IsActive,
                user.Role
            })
            .SingleOrDefaultAsync(
                context.HttpContext.RequestAborted);

        if (user is null || !user.IsActive)
        {
            context.Fail(
                "The account is inactive or no longer exists.");

            return;
        }

        if (!string.Equals(
                user.Role,
                roleClaim,
                StringComparison.Ordinal))
        {
            context.Fail(
                "Token permissions are outdated. Log in again.");
        }
    }
}

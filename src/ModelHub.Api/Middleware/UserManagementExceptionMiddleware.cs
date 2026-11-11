using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ModelHub.Api.Contracts;
using ModelHub.Application.Exceptions;

namespace ModelHub.Api.Middleware;

public sealed class UserManagementExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public UserManagementExceptionMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UserManagementException exception)
            when (!context.Response.HasStarted)
        {
            var status = exception.Error switch
            {
                UserManagementError.NotFound =>
                    StatusCodes.Status404NotFound,

                UserManagementError.DuplicateEmail =>
                    StatusCodes.Status409Conflict,

                _ =>
                    StatusCodes.Status400BadRequest
            };

            var response = new ErrorResponse(
                status,
                exception.Message,
                context.TraceIdentifier);

            context.Response.Clear();
            context.Response.StatusCode = status;

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}

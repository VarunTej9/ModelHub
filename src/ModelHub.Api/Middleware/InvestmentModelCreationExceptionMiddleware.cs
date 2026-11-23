using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ModelHub.Api.Contracts;
using ModelHub.Application.Exceptions;
using DomainValidationException =
    ModelHub.Domain.Exceptions.ValidationException;

namespace ModelHub.Api.Middleware;

public sealed class InvestmentModelCreationExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public InvestmentModelCreationExceptionMiddleware(
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
        catch (DomainValidationException exception)
            when (!context.Response.HasStarted)
        {
            context.Response.Clear();

            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(
                new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    exception.Message,
                    context.TraceIdentifier,
                    exception.Errors));
        }
        catch (DuplicateInvestmentModelNameException exception)
            when (!context.Response.HasStarted)
        {
            context.Response.Clear();

            context.Response.StatusCode =
                StatusCodes.Status409Conflict;

            await context.Response.WriteAsJsonAsync(
                new ErrorResponse(
                    StatusCodes.Status409Conflict,
                    exception.Message,
                    context.TraceIdentifier,
                    new Dictionary<string, string[]>
                    {
                        ["Name"] = new[]
                        {
                            exception.Message
                        }
                    }));
        }
    }
}

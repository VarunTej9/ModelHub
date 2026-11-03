using ModelHub.Api.Contracts;
using ModelHub.Domain.Exceptions;
using System.Numerics;

namespace ModelHub.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(httpContext, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        var traceId = httpContext.TraceIdentifier;

        var error = exception switch
        {
            ValidationException validationException =>
                new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    validationException.Message,
                    traceId,
                    validationException.Errors),

            EntityNotFoundException =>
                new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    exception.Message,
                    traceId),

            BusinessRuleException =>
                new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    exception.Message,
                    traceId),

            UnauthorizedAccessException =>
                new ErrorResponse(
                    StatusCodes.Status401Unauthorized,
                    "Authentication is required.",
                    traceId),
                        InvalidCredentialsException =>
                         
                new ErrorResponse(
                StatusCodes.Status401Unauthorized,
                exception.Message,
                traceId),


                        _ =>
                new ErrorResponse(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.",
                    traceId)

        };

        if (error.Status >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception. TraceId: {TraceId}",
                traceId);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Handled application exception. TraceId: {TraceId}",
                traceId);
        }

        httpContext.Response.Clear();
        httpContext.Response.StatusCode = error.Status;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(error);
    }
}

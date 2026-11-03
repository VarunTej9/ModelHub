namespace ModelHub.Api.Contracts;

public sealed record ErrorResponse(
    int Status,
    string Message,
    string TraceId,
    IReadOnlyDictionary<string, string[]>? Errors = null);

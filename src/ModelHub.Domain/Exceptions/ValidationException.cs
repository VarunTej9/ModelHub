namespace ModelHub.Domain.Exceptions;

public sealed class ValidationException : ModelHubException
{
    public ValidationException(
        string message,
        IReadOnlyDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}

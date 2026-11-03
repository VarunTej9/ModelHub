namespace ModelHub.Domain.Exceptions;

public abstract class ModelHubException : Exception
{
    protected ModelHubException(string message)
        : base(message)
    {
    }
}

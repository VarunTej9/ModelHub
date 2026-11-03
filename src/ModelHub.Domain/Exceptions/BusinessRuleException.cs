namespace ModelHub.Domain.Exceptions;

public sealed class BusinessRuleException : ModelHubException
{
    public BusinessRuleException(string message)
        : base(message)
    {
    }
}

namespace ModelHub.Domain.Exceptions;

public sealed class InvalidCredentialsException : ModelHubException
{
    public InvalidCredentialsException() : base("Invalid email or password.")
    {
    }
}

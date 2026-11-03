namespace ModelHub.Domain.Exceptions;

public sealed class EntityNotFoundException : ModelHubException
{
    public EntityNotFoundException(string entityName, int id)
        : base($"{entityName} with ID {id} was not found.")
    {
    }
}

using System;

namespace ModelHub.Application.Exceptions;

public sealed class DuplicateInvestmentModelNameException : Exception
{
    public DuplicateInvestmentModelNameException(
        Exception? innerException = null)
        : base(
            "An active model with this name already exists.",
            innerException)
    {
    }
}

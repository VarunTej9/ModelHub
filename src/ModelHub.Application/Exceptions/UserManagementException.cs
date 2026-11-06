using System;

namespace ModelHub.Application.Exceptions;

public enum UserManagementError
{
    Validation,
    NotFound,
    DuplicateEmail
}

public sealed class UserManagementException : Exception
{
    public UserManagementException(
        UserManagementError error,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Error = error;
    }

    public UserManagementError Error { get; }
}

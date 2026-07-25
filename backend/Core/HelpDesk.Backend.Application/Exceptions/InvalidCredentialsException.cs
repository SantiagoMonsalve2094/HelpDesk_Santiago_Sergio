using HelpDesk.Backend.Application.Resources;

namespace HelpDesk.Backend.Application.Exceptions;

public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base(ApplicationMessages.InvalidCredentials)
    {
    }
}

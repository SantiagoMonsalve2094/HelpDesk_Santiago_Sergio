namespace HelpDesk.Backend.Application.Common.Exceptions;

public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("El email o la contraseña son incorrectos, o el usuario está inactivo.")
    {
    }
}

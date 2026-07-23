using HelpDesk.Backend.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace HelpDesk.Backend.Infrastructure.Security;

internal sealed class AspNetCorePasswordHasher : IPasswordHasher
{
    private static readonly object PasswordOwner = new();
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return _passwordHasher.HashPassword(PasswordOwner, password);
    }
}

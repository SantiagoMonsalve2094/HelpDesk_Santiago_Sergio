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

    public bool Verify(string passwordHash, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var result = _passwordHasher.VerifyHashedPassword(
            PasswordOwner,
            passwordHash,
            password);
        return result is PasswordVerificationResult.Success or
            PasswordVerificationResult.SuccessRehashNeeded;
    }
}

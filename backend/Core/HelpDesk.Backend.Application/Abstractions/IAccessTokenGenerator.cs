using HelpDesk.Backend.Domain.Users;

namespace HelpDesk.Backend.Application.Abstractions;

public sealed record AccessTokenResult(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc);

public interface IAccessTokenGenerator
{
    AccessTokenResult Generate(User user);
}

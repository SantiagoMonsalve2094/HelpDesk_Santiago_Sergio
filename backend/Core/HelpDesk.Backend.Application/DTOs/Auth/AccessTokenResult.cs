namespace HelpDesk.Backend.Application.DTOs.Auth;

public sealed record AccessTokenResult(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc);

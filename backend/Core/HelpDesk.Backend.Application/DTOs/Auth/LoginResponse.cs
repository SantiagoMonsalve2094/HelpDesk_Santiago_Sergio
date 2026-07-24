using HelpDesk.Backend.Application.DTOs.Users;
using HelpDesk.Backend.Application.Features.Users;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Users;

namespace HelpDesk.Backend.Application.DTOs.Auth;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAtUtc,
    AuthenticatedUserResponse User);

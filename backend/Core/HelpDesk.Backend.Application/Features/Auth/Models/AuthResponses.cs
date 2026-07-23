using HelpDesk.Backend.Application.Features.Users.Models;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Users;

namespace HelpDesk.Backend.Application.Features.Auth.Models;

public sealed record AuthenticatedUserResponse(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    TechnicianProfileResponse? TechnicianProfile,
    SupervisorProfileResponse? SupervisorProfile);

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAtUtc,
    AuthenticatedUserResponse User);

internal static class AuthUserMapper
{
    internal static AuthenticatedUserResponse ToResponse(User user) =>
        new(
            user.Id,
            user.FullName,
            user.Email.Value,
            user.Role,
            user.TechnicianProfile is null
                ? null
                : new TechnicianProfileResponse(
                    user.TechnicianProfile.SupportCategoryIds,
                    user.TechnicianProfile.MaxActiveTickets),
            user.SupervisorProfile is null
                ? null
                : new SupervisorProfileResponse(user.SupervisorProfile.SupportCategoryId));
}

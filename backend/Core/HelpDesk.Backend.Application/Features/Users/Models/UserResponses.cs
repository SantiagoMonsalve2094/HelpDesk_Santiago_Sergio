using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Users;

namespace HelpDesk.Backend.Application.Features.Users.Models;

public sealed record TechnicianProfileResponse(
    IReadOnlyCollection<Guid> SupportCategoryIds,
    int MaxActiveTickets);

public sealed record SupervisorProfileResponse(Guid SupportCategoryId);

public sealed record UserSummaryResponse(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive);

public sealed record UserDetailsResponse(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive,
    TechnicianProfileResponse? TechnicianProfile,
    SupervisorProfileResponse? SupervisorProfile,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

internal static class UserMapper
{
    internal static UserDetailsResponse ToDetails(User user) =>
        new(
            user.Id,
            user.FullName,
            user.Email.Value,
            user.Role,
            user.IsActive,
            user.TechnicianProfile is null
                ? null
                : new TechnicianProfileResponse(
                    user.TechnicianProfile.SupportCategoryIds,
                    user.TechnicianProfile.MaxActiveTickets),
            user.SupervisorProfile is null
                ? null
                : new SupervisorProfileResponse(user.SupervisorProfile.SupportCategoryId),
            user.CreatedAtUtc,
            user.UpdatedAtUtc);
}

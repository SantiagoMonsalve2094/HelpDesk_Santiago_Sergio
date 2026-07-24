using HelpDesk.Backend.Application.DTOs.Auth;
using HelpDesk.Backend.Application.DTOs.Users;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Users;

namespace HelpDesk.Backend.Application.Features.Auth;

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

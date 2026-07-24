using HelpDesk.Backend.Application.DTOs.Users;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Users;

namespace HelpDesk.Backend.Application.Features.Users;

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

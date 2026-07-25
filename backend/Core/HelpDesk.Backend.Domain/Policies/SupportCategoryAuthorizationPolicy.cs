using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Users;

namespace HelpDesk.Backend.Domain.Policies;

public static class SupportCategoryAuthorizationPolicy
{
    public static bool CanCreateCategory(User actor) =>
        actor.IsActive && actor.Role == UserRole.SuperAdmin;

    public static bool CanConfigureSla(User actor, Guid supportCategoryId) =>
        actor.IsActive &&
        (actor.Role == UserRole.SuperAdmin ||
         actor.Role == UserRole.Supervisor &&
         actor.SupervisorProfile?.SupportCategoryId == supportCategoryId);
}

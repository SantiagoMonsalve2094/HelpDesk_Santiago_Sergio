using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Users;

namespace HelpDesk.Backend.Domain.Policies;

public static class UserProvisioningPolicy
{
    public static void EnsureCanCreate(
        User actor,
        UserRole targetRole,
        IReadOnlyCollection<Guid> targetSupportCategoryIds)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(targetSupportCategoryIds);
        EnsureTargetProfile(targetRole, targetSupportCategoryIds);

        if (!actor.IsActive)
        {
            throw new DomainException("ACTOR_INACTIVE", "Un usuario inactivo no puede crear cuentas.");
        }

        if (actor.Role == UserRole.SuperAdmin)
        {
            return;
        }

        if (actor.Role != UserRole.Supervisor || actor.SupervisorProfile is null)
        {
            throw new DomainException("USER_CREATION_FORBIDDEN", "El usuario no tiene permiso para crear cuentas.");
        }

        if (targetRole == UserRole.User)
        {
            return;
        }

        if (targetRole == UserRole.Technician &&
            targetSupportCategoryIds.Count == 1 &&
            targetSupportCategoryIds.Contains(actor.SupervisorProfile.SupportCategoryId))
        {
            return;
        }

        throw new DomainException(
            "USER_CREATION_OUTSIDE_SCOPE",
            "El supervisor solo puede crear usuarios normales o técnicos de su categoría.");
    }

    private static void EnsureTargetProfile(
        UserRole targetRole,
        IReadOnlyCollection<Guid> targetSupportCategoryIds)
    {
        if (targetSupportCategoryIds.Any(id => id == Guid.Empty))
        {
            throw new DomainException("SUPPORT_CATEGORY_REQUIRED", "Las categorías objetivo deben ser válidas.");
        }

        var valid = targetRole switch
        {
            UserRole.User or UserRole.SuperAdmin => targetSupportCategoryIds.Count == 0,
            UserRole.Technician => targetSupportCategoryIds.Count > 0,
            UserRole.Supervisor => targetSupportCategoryIds.Count == 1,
            _ => false
        };

        if (!valid)
        {
            throw new DomainException("INVALID_TARGET_USER_PROFILE", "Las categorías no corresponden al rol que se desea crear.");
        }
    }
}

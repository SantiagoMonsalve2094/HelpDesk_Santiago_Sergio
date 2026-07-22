using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Tickets;
using HelpDesk.Backend.Domain.Users;

namespace HelpDesk.Backend.Domain.Policies;

public static class TicketAccessPolicy
{
    public static bool CanCreateTicket(User actor) => actor.IsActive;

    public static bool CanView(User actor, Ticket ticket)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(ticket);

        if (!actor.IsActive || ticket.IsDeleted)
        {
            return false;
        }

        if (actor.Role == UserRole.SuperAdmin || actor.Id == ticket.CreatorUserId)
        {
            return true;
        }

        if (actor.Role == UserRole.Supervisor)
        {
            return actor.SupervisorProfile?.SupportCategoryId == ticket.SupportCategoryId;
        }

        return actor.Role == UserRole.Technician && ticket.CurrentTechnicianUserId == actor.Id;
    }

    public static bool CanComment(User actor, Ticket ticket) => CanView(actor, ticket);

    public static bool CanManageAssignment(User actor, Ticket ticket)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(ticket);

        return actor.IsActive &&
               (actor.Role == UserRole.SuperAdmin ||
                actor.Role == UserRole.Supervisor &&
                actor.SupervisorProfile?.SupportCategoryId == ticket.SupportCategoryId);
    }

    public static bool CanStartOrResolve(User actor, Ticket ticket)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(ticket);

        if (!actor.IsActive)
        {
            return false;
        }

        if (actor.Role == UserRole.SuperAdmin ||
            actor.Role == UserRole.Supervisor &&
            actor.SupervisorProfile?.SupportCategoryId == ticket.SupportCategoryId)
        {
            return true;
        }

        return actor.Role == UserRole.Technician && ticket.CurrentTechnicianUserId == actor.Id;
    }

    public static bool CanForceTransition(User actor, Ticket ticket) =>
        CanManageAssignment(actor, ticket);
}

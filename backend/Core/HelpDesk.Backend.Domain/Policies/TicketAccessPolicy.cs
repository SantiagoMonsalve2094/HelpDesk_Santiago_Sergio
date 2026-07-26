using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;
using HelpDesk.Backend.Domain.Aggregates.Users;

namespace HelpDesk.Backend.Domain.Policies;

public static class TicketAccessPolicy
{
    public static bool CanCreateTicket(User actor) =>
        actor.IsActive && actor.Role != UserRole.Technician;

    public static bool CanView(User actor, Ticket ticket)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(ticket);

        if (!actor.IsActive || ticket.IsDeleted)
        {
            return false;
        }

        if (actor.Role == UserRole.SuperAdmin || actor.Role == UserRole.Supervisor)
        {
            return true;
        }

        return actor.Role switch
        {
            UserRole.User => actor.Id == ticket.CreatorUserId,
            UserRole.Technician => ticket.CurrentTechnicianUserId == actor.Id,
            _ => false
        };
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

        return actor.Role == UserRole.Technician && ticket.CurrentTechnicianUserId == actor.Id;
    }

    public static bool CanForceTransition(User actor, Ticket ticket) =>
        CanManageAssignment(actor, ticket);
}

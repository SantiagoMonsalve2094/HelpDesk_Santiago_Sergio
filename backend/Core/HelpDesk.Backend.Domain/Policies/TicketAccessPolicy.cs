using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;
using HelpDesk.Backend.Domain.Aggregates.Users;

namespace HelpDesk.Backend.Domain.Policies;

public static class TicketAccessPolicy
{
<<<<<<< HEAD
    public static bool CanCreateTicket(User actor) =>
        actor.IsActive && actor.Role != UserRole.Technician;
=======
    public static bool CanCreateTicket(User actor) => actor.IsActive;
>>>>>>> 60bd3aa8c163527f2e018e15a29114b99aa06847

    public static bool CanView(User actor, Ticket ticket)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(ticket);

        if (!actor.IsActive || ticket.IsDeleted)
        {
            return false;
        }

<<<<<<< HEAD
        if (actor.Role == UserRole.SuperAdmin || actor.Role == UserRole.Supervisor)
=======
        if (actor.Role == UserRole.SuperAdmin || actor.Id == ticket.CreatorUserId)
>>>>>>> 60bd3aa8c163527f2e018e15a29114b99aa06847
        {
            return true;
        }

<<<<<<< HEAD
        return actor.Role switch
        {
            UserRole.User => actor.Id == ticket.CreatorUserId,
            UserRole.Technician => ticket.CurrentTechnicianUserId == actor.Id,
            _ => false
        };
=======
        if (actor.Role == UserRole.Supervisor)
        {
            return actor.SupervisorProfile?.SupportCategoryId == ticket.SupportCategoryId;
        }

        return actor.Role == UserRole.Technician && ticket.CurrentTechnicianUserId == actor.Id;
>>>>>>> 60bd3aa8c163527f2e018e15a29114b99aa06847
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

<<<<<<< HEAD
=======
        if (actor.Role == UserRole.SuperAdmin ||
            actor.Role == UserRole.Supervisor &&
            actor.SupervisorProfile?.SupportCategoryId == ticket.SupportCategoryId)
        {
            return true;
        }

>>>>>>> 60bd3aa8c163527f2e018e15a29114b99aa06847
        return actor.Role == UserRole.Technician && ticket.CurrentTechnicianUserId == actor.Id;
    }

    public static bool CanForceTransition(User actor, Ticket ticket) =>
        CanManageAssignment(actor, ticket);
}

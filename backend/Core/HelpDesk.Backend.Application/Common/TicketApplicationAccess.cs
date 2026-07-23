using HelpDesk.Backend.Domain.Policies;
using HelpDesk.Backend.Domain.Tickets;
using HelpDesk.Backend.Domain.Users;

namespace HelpDesk.Backend.Application.Common;

internal static class TicketApplicationAccess
{
    internal static void EnsureCanView(User actor, Ticket ticket)
    {
        if (!TicketAccessPolicy.CanView(actor, ticket))
        {
            throw new UnauthorizedAccessException("El usuario no puede consultar este ticket.");
        }
    }

    internal static void EnsureCanManageAssignment(User actor, Ticket ticket)
    {
        if (!TicketAccessPolicy.CanManageAssignment(actor, ticket))
        {
            throw new UnauthorizedAccessException(
                "El usuario no puede administrar asignaciones de este ticket.");
        }
    }

    internal static void EnsureCanStartOrResolve(User actor, Ticket ticket)
    {
        if (!TicketAccessPolicy.CanStartOrResolve(actor, ticket))
        {
            throw new UnauthorizedAccessException(
                "El usuario no puede cambiar la atención de este ticket.");
        }
    }

    internal static void EnsureCanComment(User actor, Ticket ticket)
    {
        if (!TicketAccessPolicy.CanComment(actor, ticket))
        {
            throw new UnauthorizedAccessException("El usuario no puede comentar este ticket.");
        }
    }

    internal static void EnsureCanForceTransition(User actor, Ticket ticket)
    {
        if (!TicketAccessPolicy.CanForceTransition(actor, ticket))
        {
            throw new UnauthorizedAccessException(
                "El usuario no puede forzar el estado de este ticket.");
        }
    }
}

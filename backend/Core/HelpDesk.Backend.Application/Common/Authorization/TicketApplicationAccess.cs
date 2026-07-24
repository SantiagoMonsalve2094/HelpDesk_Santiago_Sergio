using HelpDesk.Backend.Application.Resources;
using HelpDesk.Backend.Domain.Aggregates.Tickets;
using HelpDesk.Backend.Domain.Aggregates.Users;
using HelpDesk.Backend.Domain.Policies;

namespace HelpDesk.Backend.Application.Common.Authorization;

internal static class TicketApplicationAccess
{
    internal static void EnsureCanView(User actor, Ticket ticket)
    {
        if (!TicketAccessPolicy.CanView(actor, ticket))
        {
            throw new UnauthorizedAccessException(ApplicationMessages.CannotViewTicket);
        }
    }

    internal static void EnsureCanManageAssignment(User actor, Ticket ticket)
    {
        if (!TicketAccessPolicy.CanManageAssignment(actor, ticket))
        {
            throw new UnauthorizedAccessException(
                ApplicationMessages.CannotManageTicketAssignment);
        }
    }

    internal static void EnsureCanStartOrResolve(User actor, Ticket ticket)
    {
        if (!TicketAccessPolicy.CanStartOrResolve(actor, ticket))
        {
            throw new UnauthorizedAccessException(
                ApplicationMessages.CannotStartOrResolveTicket);
        }
    }

    internal static void EnsureCanComment(User actor, Ticket ticket)
    {
        if (!TicketAccessPolicy.CanComment(actor, ticket))
        {
            throw new UnauthorizedAccessException(ApplicationMessages.CannotCommentTicket);
        }
    }

    internal static void EnsureCanForceTransition(User actor, Ticket ticket)
    {
        if (!TicketAccessPolicy.CanForceTransition(actor, ticket))
        {
            throw new UnauthorizedAccessException(
                ApplicationMessages.CannotForceTicketStatus);
        }
    }
}

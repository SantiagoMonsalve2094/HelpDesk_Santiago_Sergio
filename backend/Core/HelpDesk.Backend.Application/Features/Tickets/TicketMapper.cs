using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;

namespace HelpDesk.Backend.Application.Features.Tickets;

internal static class TicketMapper
{
    internal static TicketDetailsResponse ToDetails(Ticket ticket) =>
        new(
            ticket.Id,
            ticket.Number.Value,
            ticket.Subject,
            ticket.Description,
            ticket.CreatorUserId,
            ticket.SupportCategoryId,
            ticket.Priority,
            ticket.Status,
            ticket.CurrentTechnicianUserId,
            ticket.IsOverdue,
            ticket.CreatedAtUtc,
            ticket.UpdatedAtUtc,
            ticket.ResolvedAtUtc,
            ticket.ClosedAtUtc,
            ticket.Assignments.Select(assignment => new TicketAssignmentResponse(
                assignment.Id,
                assignment.TechnicianUserId,
                assignment.AssignedByUserId,
                assignment.AssignedAtUtc,
                assignment.EndedAtUtc,
                assignment.Reason,
                assignment.IsCurrent)).ToArray(),
            ticket.Comments.Select(comment => new TicketCommentResponse(
                comment.Id,
                comment.AuthorUserId,
                comment.Type,
                comment.Body,
                comment.SatisfiesResolutionRequirement,
                comment.CreatedAtUtc)).ToArray(),
            ticket.StatusHistory.Select(change => new TicketStatusChangeResponse(
                change.Id,
                change.PreviousStatus,
                change.NewStatus,
                change.ChangedByUserId,
                change.Reason,
                change.IsAutomatic,
                change.ChangedAtUtc)).ToArray(),
            ticket.SlaCycles.Select(cycle => new TicketSlaCycleResponse(
                cycle.Id,
                cycle.Trigger,
                cycle.SupportCategoryId,
                cycle.Priority,
                cycle.Duration,
                cycle.StartedAtUtc,
                cycle.DeadlineAtUtc,
                cycle.RespondedAtUtc,
                cycle.BreachedAtUtc,
                cycle.ResponsibleTechnicianUserId,
                cycle.Outcome)).ToArray());
}

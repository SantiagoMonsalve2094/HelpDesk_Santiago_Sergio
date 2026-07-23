using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Tickets;

namespace HelpDesk.Backend.Application.Features.Tickets.Models;

public sealed record CreatedTicketResponse(Guid TicketId, string TicketNumber);

public sealed record TicketAssignmentResponse(
    Guid Id,
    Guid TechnicianUserId,
    Guid AssignedByUserId,
    DateTimeOffset AssignedAtUtc,
    DateTimeOffset? EndedAtUtc,
    string? Reason,
    bool IsCurrent);

public sealed record TicketCommentResponse(
    Guid Id,
    Guid AuthorUserId,
    TicketCommentType Type,
    string Body,
    bool SatisfiesResolutionRequirement,
    DateTimeOffset CreatedAtUtc);

public sealed record TicketStatusChangeResponse(
    Guid Id,
    TicketStatus? PreviousStatus,
    TicketStatus NewStatus,
    Guid? ChangedByUserId,
    string? Reason,
    bool IsAutomatic,
    DateTimeOffset ChangedAtUtc);

public sealed record TicketSlaCycleResponse(
    Guid Id,
    SlaCycleTrigger Trigger,
    Guid SupportCategoryId,
    TicketPriority Priority,
    TimeSpan Duration,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset DeadlineAtUtc,
    DateTimeOffset? RespondedAtUtc,
    DateTimeOffset? BreachedAtUtc,
    Guid? ResponsibleTechnicianUserId,
    SlaOutcome Outcome);

public sealed record TicketSummaryResponse(
    Guid Id,
    string TicketNumber,
    string Subject,
    Guid CreatorUserId,
    Guid SupportCategoryId,
    TicketPriority Priority,
    TicketStatus Status,
    Guid? CurrentTechnicianUserId,
    bool IsOverdue,
    DateTimeOffset SlaDeadlineAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record TicketDetailsResponse(
    Guid Id,
    string TicketNumber,
    string Subject,
    string Description,
    Guid CreatorUserId,
    Guid SupportCategoryId,
    TicketPriority Priority,
    TicketStatus Status,
    Guid? CurrentTechnicianUserId,
    bool IsOverdue,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? ResolvedAtUtc,
    DateTimeOffset? ClosedAtUtc,
    IReadOnlyCollection<TicketAssignmentResponse> Assignments,
    IReadOnlyCollection<TicketCommentResponse> Comments,
    IReadOnlyCollection<TicketStatusChangeResponse> StatusHistory,
    IReadOnlyCollection<TicketSlaCycleResponse> SlaCycles);

public sealed record AssignableTechnicianResponse(
    Guid TechnicianUserId,
    string FullName,
    int MaxActiveTickets,
    int ActiveTicketCount,
    int AvailableCapacity);

public sealed record SlaAlertResponse(
    Guid TicketId,
    string TicketNumber,
    string Subject,
    Guid SupportCategoryId,
    Guid? CurrentTechnicianUserId,
    TicketPriority Priority,
    TicketStatus Status,
    DateTimeOffset DeadlineAtUtc,
    TimeSpan RemainingTime,
    bool IsBreached);

public sealed record SlaComplianceGroupResponse(
    Guid SupportCategoryId,
    string SupportCategoryName,
    Guid? TechnicianUserId,
    string TechnicianName,
    int MetCycles,
    int BreachedCycles,
    int PendingCycles,
    int EvaluatedCycles,
    decimal? CompliancePercentage);

public sealed record SlaReportResponse(
    IReadOnlyList<SlaComplianceGroupResponse> Groups,
    int TotalMetCycles,
    int TotalBreachedCycles,
    int TotalPendingCycles,
    int TotalEvaluatedCycles,
    decimal? CompliancePercentage);

public static class SlaReportLabels
{
    public const string UnassignedTechnician = "Sin técnico";
}

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

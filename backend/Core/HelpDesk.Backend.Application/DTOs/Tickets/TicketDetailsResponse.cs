using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;

namespace HelpDesk.Backend.Application.DTOs.Tickets;

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

using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;

namespace HelpDesk.Backend.Application.DTOs.Tickets;

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

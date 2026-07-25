using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;

namespace HelpDesk.Backend.Application.DTOs.Sla;

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

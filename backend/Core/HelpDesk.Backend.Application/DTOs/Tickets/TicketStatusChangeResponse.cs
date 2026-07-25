using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;

namespace HelpDesk.Backend.Application.DTOs.Tickets;

public sealed record TicketStatusChangeResponse(
    Guid Id,
    TicketStatus? PreviousStatus,
    TicketStatus NewStatus,
    Guid? ChangedByUserId,
    string? Reason,
    bool IsAutomatic,
    DateTimeOffset ChangedAtUtc);

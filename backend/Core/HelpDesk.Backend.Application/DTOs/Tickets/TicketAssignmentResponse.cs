using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;

namespace HelpDesk.Backend.Application.DTOs.Tickets;

public sealed record TicketAssignmentResponse(
    Guid Id,
    Guid TechnicianUserId,
    Guid AssignedByUserId,
    DateTimeOffset AssignedAtUtc,
    DateTimeOffset? EndedAtUtc,
    string? Reason,
    bool IsCurrent);

using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;

namespace HelpDesk.Backend.Application.DTOs.Tickets;

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

using HelpDesk.Backend.Domain.Common;

namespace HelpDesk.Backend.Domain.DomainEvents;

public sealed record TicketSlaBreachedDomainEvent(
    Guid TicketId,
    Guid SlaCycleId,
    Guid SupportCategoryId,
    Guid? ResponsibleTechnicianUserId,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;

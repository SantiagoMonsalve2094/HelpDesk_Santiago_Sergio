using HelpDesk.Backend.Domain.Common;

namespace HelpDesk.Backend.Domain.DomainEvents;

public sealed record TicketClosedDomainEvent(
    Guid TicketId,
    Guid? ClosedByUserId,
    bool IsAutomatic,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;

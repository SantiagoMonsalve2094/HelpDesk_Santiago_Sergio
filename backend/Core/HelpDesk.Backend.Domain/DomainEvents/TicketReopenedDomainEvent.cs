using HelpDesk.Backend.Domain.Common;

namespace HelpDesk.Backend.Domain.DomainEvents;

public sealed record TicketReopenedDomainEvent(
    Guid TicketId,
    Guid? TechnicianUserId,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;

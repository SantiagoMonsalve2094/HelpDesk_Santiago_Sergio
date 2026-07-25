using HelpDesk.Backend.Domain.Common;

namespace HelpDesk.Backend.Domain.DomainEvents;

public sealed record TicketReassignedDomainEvent(
    Guid TicketId,
    Guid PreviousTechnicianUserId,
    Guid NewTechnicianUserId,
    Guid ReassignedByUserId,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;

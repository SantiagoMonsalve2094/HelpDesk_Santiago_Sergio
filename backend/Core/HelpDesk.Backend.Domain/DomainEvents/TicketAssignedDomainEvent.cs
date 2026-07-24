using HelpDesk.Backend.Domain.Common;

namespace HelpDesk.Backend.Domain.DomainEvents;

public sealed record TicketAssignedDomainEvent(
    Guid TicketId,
    Guid TechnicianUserId,
    Guid AssignedByUserId,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;

using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Domain.DomainEvents;

public sealed record TicketStatusChangedDomainEvent(
    Guid TicketId,
    TicketStatus PreviousStatus,
    TicketStatus NewStatus,
    Guid? ChangedByUserId,
    bool IsAutomatic,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;

using HelpDesk.Backend.Domain.Common;

namespace HelpDesk.Backend.Domain.DomainEvents;

public sealed record TicketCreatedDomainEvent(
    Guid TicketId,
    Guid CreatorUserId,
    Guid SupportCategoryId,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;

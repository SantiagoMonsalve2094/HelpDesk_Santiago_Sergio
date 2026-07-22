using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Domain.DomainEvents;

public sealed record TicketCreatedDomainEvent(
    Guid TicketId,
    Guid CreatorUserId,
    Guid SupportCategoryId,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;

public sealed record TicketAssignedDomainEvent(
    Guid TicketId,
    Guid TechnicianUserId,
    Guid AssignedByUserId,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;

public sealed record TicketReassignedDomainEvent(
    Guid TicketId,
    Guid PreviousTechnicianUserId,
    Guid NewTechnicianUserId,
    Guid ReassignedByUserId,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;

public sealed record TicketStatusChangedDomainEvent(
    Guid TicketId,
    TicketStatus PreviousStatus,
    TicketStatus NewStatus,
    Guid? ChangedByUserId,
    bool IsAutomatic,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;

public sealed record TicketSlaBreachedDomainEvent(
    Guid TicketId,
    Guid SlaCycleId,
    Guid SupportCategoryId,
    Guid? ResponsibleTechnicianUserId,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;

public sealed record TicketReopenedDomainEvent(
    Guid TicketId,
    Guid? TechnicianUserId,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;

public sealed record TicketClosedDomainEvent(
    Guid TicketId,
    Guid? ClosedByUserId,
    bool IsAutomatic,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;

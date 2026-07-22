using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Domain.Tickets;

public sealed class TicketStatusChange : Entity
{
    internal TicketStatusChange(
        Guid id,
        TicketStatus? previousStatus,
        TicketStatus newStatus,
        Guid? changedByUserId,
        string? reason,
        bool isAutomatic,
        DateTimeOffset changedAtUtc)
        : base(id)
    {
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        ChangedByUserId = changedByUserId;
        Reason = reason;
        IsAutomatic = isAutomatic;
        ChangedAtUtc = changedAtUtc;
    }

    public TicketStatus? PreviousStatus { get; }
    public TicketStatus NewStatus { get; }
    public Guid? ChangedByUserId { get; }
    public string? Reason { get; }
    public bool IsAutomatic { get; }
    public DateTimeOffset ChangedAtUtc { get; }
}

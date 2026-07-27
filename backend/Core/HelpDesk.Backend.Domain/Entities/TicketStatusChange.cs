using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Domain.Entities.Tickets;

public sealed class TicketStatusChange : Entity
{
    private TicketStatusChange()
    {
    }

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

    public TicketStatus? PreviousStatus { get; private set; }
    public TicketStatus NewStatus { get; private set; }
    public Guid? ChangedByUserId { get; private set; }
    public string? Reason { get; private set; }
    public bool IsAutomatic { get; private set; }
    public DateTimeOffset ChangedAtUtc { get; private set; }
}

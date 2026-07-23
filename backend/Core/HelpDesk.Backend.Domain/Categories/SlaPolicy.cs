using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Domain.Categories;

public sealed class SlaPolicy : Entity
{
    private SlaPolicy()
    {
    }

    internal SlaPolicy(Guid id, TicketPriority priority, TimeSpan responseTime)
        : base(id)
    {
        Priority = priority;
        ResponseTime = Guard.PositiveDuration(
            responseTime,
            "INVALID_SLA_DURATION",
            "La duración del SLA debe ser mayor que cero.");
    }

    public TicketPriority Priority { get; private set; }
    public TimeSpan ResponseTime { get; private set; }

    internal void Update(TimeSpan responseTime)
    {
        ResponseTime = Guard.PositiveDuration(
            responseTime,
            "INVALID_SLA_DURATION",
            "La duración del SLA debe ser mayor que cero.");
    }
}

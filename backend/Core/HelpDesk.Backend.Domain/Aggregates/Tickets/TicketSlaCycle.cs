using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Domain.Aggregates.Tickets;

public sealed class TicketSlaCycle : Entity
{
    private TicketSlaCycle()
    {
    }

    internal TicketSlaCycle(
        Guid id,
        SlaCycleTrigger trigger,
        Guid supportCategoryId,
        TicketPriority priority,
        TimeSpan duration,
        DateTimeOffset startedAtUtc)
        : base(id)
    {
        SupportCategoryId = Guard.Required(
            supportCategoryId,
            "SUPPORT_CATEGORY_REQUIRED",
            "La categoría del ciclo SLA es obligatoria.");
        Trigger = trigger;
        Priority = priority;
        Duration = Guard.PositiveDuration(
            duration,
            "INVALID_SLA_DURATION",
            "La duración del SLA debe ser mayor que cero.");
        StartedAtUtc = startedAtUtc;
        DeadlineAtUtc = startedAtUtc.Add(Duration);
        Outcome = SlaOutcome.Pending;
    }

    public SlaCycleTrigger Trigger { get; private set; }
    public Guid SupportCategoryId { get; private set; }
    public TicketPriority Priority { get; private set; }
    public TimeSpan Duration { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset DeadlineAtUtc { get; private set; }
    public DateTimeOffset? RespondedAtUtc { get; private set; }
    public DateTimeOffset? BreachedAtUtc { get; private set; }
    public Guid? ResponsibleTechnicianUserId { get; private set; }
    public SlaOutcome Outcome { get; private set; }
    public bool IsPending => Outcome == SlaOutcome.Pending;

    internal bool Evaluate(DateTimeOffset now, Guid? currentTechnicianUserId)
    {
        if (!IsPending || now <= DeadlineAtUtc)
        {
            return false;
        }

        Outcome = SlaOutcome.Breached;
        BreachedAtUtc = now;
        ResponsibleTechnicianUserId = currentTechnicianUserId;
        return true;
    }

    internal void RecordResponse(
        Guid technicianUserId,
        Guid? technicianUserIdAtDeadline,
        DateTimeOffset now)
    {
        Guard.Required(
            technicianUserId,
            "TECHNICIAN_REQUIRED",
            "El técnico que responde es obligatorio.");

        if (RespondedAtUtc is not null)
        {
            return;
        }

        RespondedAtUtc = now;

        if (Outcome == SlaOutcome.Breached)
        {
            return;
        }

        if (now <= DeadlineAtUtc)
        {
            Outcome = SlaOutcome.Met;
            ResponsibleTechnicianUserId = technicianUserId;
            return;
        }

        Outcome = SlaOutcome.Breached;
        BreachedAtUtc = now;
        ResponsibleTechnicianUserId = technicianUserIdAtDeadline;
    }
}

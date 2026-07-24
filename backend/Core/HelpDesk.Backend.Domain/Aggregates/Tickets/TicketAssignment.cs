using HelpDesk.Backend.Domain.Common;

namespace HelpDesk.Backend.Domain.Aggregates.Tickets;

public sealed class TicketAssignment : Entity
{
    private TicketAssignment()
    {
    }

    internal TicketAssignment(
        Guid id,
        Guid technicianUserId,
        Guid assignedByUserId,
        DateTimeOffset assignedAtUtc,
        string? reason)
        : base(id)
    {
        TechnicianUserId = Guard.Required(
            technicianUserId,
            "TECHNICIAN_REQUIRED",
            "El técnico es obligatorio.");
        AssignedByUserId = Guard.Required(
            assignedByUserId,
            "ASSIGNER_REQUIRED",
            "El usuario que realiza la asignación es obligatorio.");
        AssignedAtUtc = assignedAtUtc;
        Reason = reason;
    }

    public Guid TechnicianUserId { get; private set; }
    public Guid AssignedByUserId { get; private set; }
    public DateTimeOffset AssignedAtUtc { get; private set; }
    public DateTimeOffset? EndedAtUtc { get; private set; }
    public string? Reason { get; private set; }
    public bool IsCurrent => EndedAtUtc is null;

    internal void End(DateTimeOffset now)
    {
        if (EndedAtUtc is not null)
        {
            throw new DomainException("ASSIGNMENT_ALREADY_ENDED", "La asignación ya había finalizado.");
        }

        EndedAtUtc = now;
    }
}

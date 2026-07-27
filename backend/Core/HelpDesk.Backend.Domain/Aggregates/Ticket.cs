using System.Collections.ObjectModel;
using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.DomainEvents;
using HelpDesk.Backend.Domain.Entities.Tickets;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.ValueObjects;

namespace HelpDesk.Backend.Domain.Aggregates.Tickets;

public sealed class Ticket : AggregateRoot
{
    private static readonly TimeSpan ReopenWindow = TimeSpan.FromHours(48);
    private const int SubjectMaxLength = 200;
    private const int DescriptionMaxLength = 4000;
    private const int JustificationMaxLength = 1000;

    private readonly List<TicketAssignment> _assignments = [];
    private readonly List<TicketComment> _comments = [];
    private readonly List<TicketStatusChange> _statusHistory = [];
    private readonly List<TicketSlaCycle> _slaCycles = [];

    private Ticket()
    {
        Number = null!;
        Subject = string.Empty;
        Description = string.Empty;
    }

    private Ticket(
        Guid id,
        TicketNumber number,
        string subject,
        string description,
        Guid creatorUserId,
        Guid supportCategoryId,
        TicketPriority priority,
        TimeSpan slaDuration,
        DateTimeOffset now)
        : base(id)
    {
        Number = number;
        Subject = subject;
        Description = description;
        CreatorUserId = creatorUserId;
        SupportCategoryId = supportCategoryId;
        Priority = priority;
        Status = TicketStatus.Open;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;

        _statusHistory.Add(new TicketStatusChange(
            Guid.NewGuid(),
            null,
            TicketStatus.Open,
            creatorUserId,
            null,
            false,
            now));
        _slaCycles.Add(CreateSlaCycle(SlaCycleTrigger.TicketCreation, slaDuration, now));
    }

    public TicketNumber Number { get; private set; }
    public string Subject { get; private set; }
    public string Description { get; private set; }
    public Guid CreatorUserId { get; private set; }
    public Guid SupportCategoryId { get; private set; }
    public TicketPriority Priority { get; private set; }
    public TicketStatus Status { get; private set; }
    public Guid? CurrentTechnicianUserId { get; private set; }
    public bool IsDeleted { get; private set; }
    public bool IsOverdue => CurrentSlaCycle.Outcome == SlaOutcome.Breached;
    public bool CountsTowardTechnicianCapacity =>
        CurrentTechnicianUserId is not null && Status is TicketStatus.Assigned or TicketStatus.InProgress or TicketStatus.Reopened;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? ResolvedAtUtc { get; private set; }
    public DateTimeOffset? ClosedAtUtc { get; private set; }
    public TicketSlaCycle CurrentSlaCycle => _slaCycles[^1];
    public IReadOnlyCollection<TicketAssignment> Assignments => new ReadOnlyCollection<TicketAssignment>(_assignments);
    public IReadOnlyCollection<TicketComment> Comments => new ReadOnlyCollection<TicketComment>(_comments);
    public IReadOnlyCollection<TicketStatusChange> StatusHistory => new ReadOnlyCollection<TicketStatusChange>(_statusHistory);
    public IReadOnlyCollection<TicketSlaCycle> SlaCycles => new ReadOnlyCollection<TicketSlaCycle>(_slaCycles);

    public static Ticket Create(
        TicketNumber number,
        string subject,
        string description,
        Guid creatorUserId,
        Guid supportCategoryId,
        TicketPriority priority,
        TimeSpan slaDuration,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(number);
        Guard.Required(creatorUserId, "TICKET_CREATOR_REQUIRED", "El creador del ticket es obligatorio.");
        Guard.Required(supportCategoryId, "SUPPORT_CATEGORY_REQUIRED", "La categoría del ticket es obligatoria.");
        Guard.PositiveDuration(slaDuration, "INVALID_SLA_DURATION", "La duración del SLA debe ser mayor que cero.");

        var ticket = new Ticket(
            Guid.NewGuid(),
            number,
            NormalizeSubject(subject),
            NormalizeDescription(description),
            creatorUserId,
            supportCategoryId,
            priority,
            slaDuration,
            now);

        ticket.RaiseDomainEvent(new TicketCreatedDomainEvent(
            ticket.Id,
            creatorUserId,
            supportCategoryId,
            now));
        return ticket;
    }

    public void UpdateDescription(string subject, string description, Guid requesterUserId, DateTimeOffset now)
    {
        EnsureUsable();
        EnsureCreator(requesterUserId);
        if (Status != TicketStatus.Open || _assignments.Count > 0)
        {
            throw new DomainException("TICKET_CANNOT_BE_EDITED", "El ticket solo puede editarse antes de su primera asignación.");
        }

        Subject = NormalizeSubject(subject);
        Description = NormalizeDescription(description);
        UpdatedAtUtc = now;
    }

    public void Assign(Guid technicianUserId, Guid assignedByUserId, DateTimeOffset now)
    {
        EnsureUsable();
        if (Status is not (TicketStatus.Open or TicketStatus.Reopened) || CurrentTechnicianUserId is not null)
        {
            throw new DomainException("TICKET_CANNOT_BE_ASSIGNED", "El ticket no está disponible para asignación.");
        }

        AddAssignment(technicianUserId, assignedByUserId, null, now);
        ChangeStatus(TicketStatus.Assigned, assignedByUserId, null, false, now);
        RaiseDomainEvent(new TicketAssignedDomainEvent(Id, technicianUserId, assignedByUserId, now));
    }

    public void Reassign(
        Guid newTechnicianUserId,
        Guid reassignedByUserId,
        string reason,
        DateTimeOffset now)
    {
        EnsureUsable();
        if (Status is not (TicketStatus.Assigned or TicketStatus.InProgress or TicketStatus.Reopened) ||
            CurrentTechnicianUserId is null)
        {
            throw new DomainException("TICKET_CANNOT_BE_REASSIGNED", "El ticket no tiene una asignación activa que pueda reemplazarse.");
        }

        Guard.Required(newTechnicianUserId, "TECHNICIAN_REQUIRED", "El nuevo técnico es obligatorio.");
        Guard.Required(reassignedByUserId, "REASSIGNER_REQUIRED", "El responsable de la reasignación es obligatorio.");
        var normalizedReason = NormalizeJustification(reason, "REASSIGNMENT_REASON_REQUIRED");

        if (CurrentTechnicianUserId == newTechnicianUserId)
        {
            throw new DomainException("SAME_TECHNICIAN_REASSIGNMENT", "El nuevo técnico debe ser diferente al técnico actual.");
        }

        var previousTechnicianUserId = CurrentTechnicianUserId.Value;
        EndCurrentAssignment(now);
        AddAssignment(newTechnicianUserId, reassignedByUserId, normalizedReason, now);
        AddComment(
            reassignedByUserId,
            TicketCommentType.ReassignmentReason,
            normalizedReason,
            false,
            now);

        ChangeStatus(TicketStatus.Assigned, reassignedByUserId, normalizedReason, false, now);
        RaiseDomainEvent(new TicketReassignedDomainEvent(
            Id,
            previousTechnicianUserId,
            newTechnicianUserId,
            reassignedByUserId,
            now));
    }

    public void StartProgress(Guid changedByUserId, DateTimeOffset now)
    {
        EnsureUsable();
        if (Status is not (TicketStatus.Assigned or TicketStatus.Reopened))
        {
            throw new DomainException("INVALID_STATUS_TRANSITION", "Solo un ticket asignado o reabierto puede pasar a en proceso.");
        }

        var technicianUserId = EnsureCurrentTechnician();
        EnsureAssignedTechnician(changedByUserId, technicianUserId);
        RecordSlaResponse(technicianUserId, now);
        ChangeStatus(TicketStatus.InProgress, changedByUserId, null, false, now);
    }

    public void Resolve(Guid changedByUserId, string resolutionComment, DateTimeOffset now)
    {
        EnsureUsable();
        if (Status is not (TicketStatus.InProgress or TicketStatus.Reopened))
        {
            throw new DomainException("INVALID_STATUS_TRANSITION", "Solo un ticket en proceso o reabierto puede resolverse.");
        }

        var technicianUserId = EnsureCurrentTechnician();
        EnsureAssignedTechnician(changedByUserId, technicianUserId);
        RecordSlaResponse(technicianUserId, now);
        AddComment(
            changedByUserId,
            TicketCommentType.Resolution,
            resolutionComment,
            true,
            now);
        ResolvedAtUtc = now;
        ClosedAtUtc = null;
        ChangeStatus(TicketStatus.Resolved, changedByUserId, null, false, now);
    }

    public void AddGeneralComment(Guid authorUserId, string body, DateTimeOffset now)
    {
        EnsureUsable();
        AddComment(authorUserId, TicketCommentType.General, body, false, now);
        UpdatedAtUtc = now;
    }

    public void CloseByCreator(Guid creatorUserId, DateTimeOffset now)
    {
        EnsureUsable();
        EnsureCreator(creatorUserId);
        EnsureCanCloseResolved(now, requireWindowOpen: true);
        Close(creatorUserId, false, null, now);
    }

    public void CloseAutomatically(DateTimeOffset now)
    {
        EnsureUsable();
        EnsureCanCloseResolved(now, requireWindowOpen: false);
        Close(null, true, "Cierre automático al vencer la ventana de reapertura.", now);
    }

    public void ReopenByCreator(
        Guid creatorUserId,
        bool previousTechnicianHasCapacity,
        TimeSpan slaDuration,
        DateTimeOffset now)
    {
        EnsureUsable();
        EnsureCreator(creatorUserId);
        if (Status != TicketStatus.Resolved || ResolvedAtUtc is null)
        {
            throw new DomainException("TICKET_CANNOT_BE_REOPENED", "Solo un ticket resuelto puede reabrirse.");
        }

        if (now > ResolvedAtUtc.Value.Add(ReopenWindow))
        {
            throw new DomainException("REOPEN_WINDOW_EXPIRED", "La ventana de 48 horas para reabrir el ticket venció.");
        }

        Guard.PositiveDuration(slaDuration, "INVALID_SLA_DURATION", "La duración del SLA debe ser mayor que cero.");

        if (!previousTechnicianHasCapacity && CurrentTechnicianUserId is not null)
        {
            EndCurrentAssignment(now);
            CurrentTechnicianUserId = null;
        }

        ResolvedAtUtc = null;
        ClosedAtUtc = null;
        _slaCycles.Add(CreateSlaCycle(SlaCycleTrigger.Reopening, slaDuration, now));
        ChangeStatus(TicketStatus.Reopened, creatorUserId, null, false, now);
        RaiseDomainEvent(new TicketReopenedDomainEvent(Id, CurrentTechnicianUserId, now));
    }

    public void ForceTransition(
        TicketStatus targetStatus,
        Guid administratorUserId,
        string justification,
        DateTimeOffset now,
        TimeSpan? newSlaDuration = null)
    {
        EnsureUsable();
        Guard.Required(administratorUserId, "ADMINISTRATOR_REQUIRED", "El responsable del cambio administrativo es obligatorio.");
        var normalizedJustification = NormalizeJustification(justification, "ADMINISTRATIVE_JUSTIFICATION_REQUIRED");

        if (targetStatus == Status)
        {
            throw new DomainException("STATUS_ALREADY_SET", "El ticket ya se encuentra en el estado solicitado.");
        }

        var satisfiesResolution = targetStatus is TicketStatus.Resolved or TicketStatus.Closed;
        AddComment(
            administratorUserId,
            TicketCommentType.AdministrativeJustification,
            normalizedJustification,
            satisfiesResolution,
            now);

        PrepareForcedTarget(targetStatus, newSlaDuration, now);
        ChangeStatus(targetStatus, administratorUserId, normalizedJustification, false, now);

        if (targetStatus == TicketStatus.Closed)
        {
            RaiseDomainEvent(new TicketClosedDomainEvent(Id, administratorUserId, false, now));
        }
        else if (targetStatus == TicketStatus.Reopened)
        {
            RaiseDomainEvent(new TicketReopenedDomainEvent(Id, CurrentTechnicianUserId, now));
        }
    }

    public bool EvaluateSla(DateTimeOffset now)
    {
        EnsureUsable();
        var technicianAtDeadline = FindTechnicianAssignedAt(CurrentSlaCycle.DeadlineAtUtc);
        var breached = CurrentSlaCycle.Evaluate(now, technicianAtDeadline);
        if (!breached)
        {
            return false;
        }

        UpdatedAtUtc = now;
        RaiseDomainEvent(new TicketSlaBreachedDomainEvent(
            Id,
            CurrentSlaCycle.Id,
            SupportCategoryId,
            CurrentSlaCycle.ResponsibleTechnicianUserId,
            now));
        return true;
    }

    public void DeleteByCreator(Guid creatorUserId, DateTimeOffset now)
    {
        EnsureUsable();
        EnsureCreator(creatorUserId);
        if (Status != TicketStatus.Open || _assignments.Count > 0)
        {
            throw new DomainException("TICKET_CANNOT_BE_DELETED", "El ticket solo puede eliminarse antes de su primera asignación.");
        }

        IsDeleted = true;
        UpdatedAtUtc = now;
    }

    private void PrepareForcedTarget(TicketStatus targetStatus, TimeSpan? newSlaDuration, DateTimeOffset now)
    {
        switch (targetStatus)
        {
            case TicketStatus.Open:
                EndAndClearAssignment(now);
                ResolvedAtUtc = null;
                ClosedAtUtc = null;
                EnsurePendingSlaOrCreate(newSlaDuration, SlaCycleTrigger.AdministrativeReset, now);
                break;
            case TicketStatus.Assigned:
                EnsureCurrentTechnician();
                ResolvedAtUtc = null;
                ClosedAtUtc = null;
                break;
            case TicketStatus.InProgress:
                RecordSlaResponse(EnsureCurrentTechnician(), now);
                ResolvedAtUtc = null;
                ClosedAtUtc = null;
                break;
            case TicketStatus.Resolved:
                RecordSlaResponse(EnsureCurrentTechnician(), now);
                ResolvedAtUtc = now;
                ClosedAtUtc = null;
                break;
            case TicketStatus.Closed:
                EndCurrentAssignmentIfAny(now);
                ClosedAtUtc = now;
                break;
            case TicketStatus.Reopened:
                if (newSlaDuration is null)
                {
                    throw new DomainException("SLA_DURATION_REQUIRED", "Reabrir administrativamente requiere una duración SLA.");
                }

                ResolvedAtUtc = null;
                ClosedAtUtc = null;
                _slaCycles.Add(CreateSlaCycle(SlaCycleTrigger.AdministrativeReset, newSlaDuration.Value, now));
                break;
            default:
                throw new DomainException("INVALID_TARGET_STATUS", "El estado de destino no es válido.");
        }
    }

    private void EnsurePendingSlaOrCreate(TimeSpan? newSlaDuration, SlaCycleTrigger trigger, DateTimeOffset now)
    {
        if (CurrentSlaCycle.IsPending)
        {
            return;
        }

        if (newSlaDuration is null)
        {
            throw new DomainException("SLA_DURATION_REQUIRED", "El cambio administrativo requiere una duración SLA nueva.");
        }

        _slaCycles.Add(CreateSlaCycle(trigger, newSlaDuration.Value, now));
    }

    private void Close(Guid? closedByUserId, bool isAutomatic, string? reason, DateTimeOffset now)
    {
        EndCurrentAssignmentIfAny(now);
        ClosedAtUtc = now;
        ChangeStatus(TicketStatus.Closed, closedByUserId, reason, isAutomatic, now);
        RaiseDomainEvent(new TicketClosedDomainEvent(Id, closedByUserId, isAutomatic, now));
    }

    private void EnsureCanCloseResolved(DateTimeOffset now, bool requireWindowOpen)
    {
        if (Status != TicketStatus.Resolved || ResolvedAtUtc is null)
        {
            throw new DomainException("TICKET_CANNOT_BE_CLOSED", "Solo un ticket resuelto puede cerrarse por el flujo normal.");
        }

        if (!_comments.Any(comment =>
                comment.SatisfiesResolutionRequirement &&
                comment.AuthorUserId == CurrentTechnicianUserId))
        {
            throw new DomainException("RESOLUTION_COMMENT_REQUIRED", "El ticket requiere evidencia de resolución antes de cerrarse.");
        }

        var windowEnd = ResolvedAtUtc.Value.Add(ReopenWindow);
        if (requireWindowOpen && now > windowEnd)
        {
            throw new DomainException("REOPEN_WINDOW_EXPIRED", "La ventana de cierre por el creador ya venció.");
        }

        if (!requireWindowOpen && now < windowEnd)
        {
            throw new DomainException("REOPEN_WINDOW_ACTIVE", "La ventana de reapertura todavía está activa.");
        }
    }

    private void AddAssignment(Guid technicianUserId, Guid assignedByUserId, string? reason, DateTimeOffset now)
    {
        var assignment = new TicketAssignment(
            Guid.NewGuid(),
            technicianUserId,
            assignedByUserId,
            now,
            reason);
        _assignments.Add(assignment);
        CurrentTechnicianUserId = technicianUserId;
        UpdatedAtUtc = now;
    }

    private void AddComment(
        Guid authorUserId,
        TicketCommentType type,
        string body,
        bool satisfiesResolutionRequirement,
        DateTimeOffset now)
    {
        _comments.Add(new TicketComment(
            Guid.NewGuid(),
            authorUserId,
            type,
            body,
            satisfiesResolutionRequirement,
            now));
    }

    private void ChangeStatus(
        TicketStatus newStatus,
        Guid? changedByUserId,
        string? reason,
        bool isAutomatic,
        DateTimeOffset now)
    {
        if (newStatus == Status)
        {
            UpdatedAtUtc = now;
            return;
        }

        var previousStatus = Status;
        Status = newStatus;
        UpdatedAtUtc = now;
        _statusHistory.Add(new TicketStatusChange(
            Guid.NewGuid(),
            previousStatus,
            newStatus,
            changedByUserId,
            reason,
            isAutomatic,
            now));
        RaiseDomainEvent(new TicketStatusChangedDomainEvent(
            Id,
            previousStatus,
            newStatus,
            changedByUserId,
            isAutomatic,
            now));
    }

    private void RecordSlaResponse(Guid technicianUserId, DateTimeOffset now)
    {
        var previousOutcome = CurrentSlaCycle.Outcome;
        var technicianAtDeadline = FindTechnicianAssignedAt(CurrentSlaCycle.DeadlineAtUtc);
        CurrentSlaCycle.RecordResponse(technicianUserId, technicianAtDeadline, now);

        if (previousOutcome != SlaOutcome.Breached && CurrentSlaCycle.Outcome == SlaOutcome.Breached)
        {
            RaiseDomainEvent(new TicketSlaBreachedDomainEvent(
                Id,
                CurrentSlaCycle.Id,
                SupportCategoryId,
                CurrentSlaCycle.ResponsibleTechnicianUserId,
                now));
        }
    }

    private TicketSlaCycle CreateSlaCycle(SlaCycleTrigger trigger, TimeSpan duration, DateTimeOffset now) =>
        new(Guid.NewGuid(), trigger, SupportCategoryId, Priority, duration, now);

    private Guid? FindTechnicianAssignedAt(DateTimeOffset instantUtc) =>
        _assignments
            .Where(assignment =>
                assignment.AssignedAtUtc <= instantUtc &&
                (assignment.EndedAtUtc is null || assignment.EndedAtUtc > instantUtc))
            .OrderByDescending(assignment => assignment.AssignedAtUtc)
            .Select(assignment => (Guid?)assignment.TechnicianUserId)
            .FirstOrDefault();

    private Guid EnsureCurrentTechnician() =>
        CurrentTechnicianUserId ?? throw new DomainException("TECHNICIAN_REQUIRED", "El estado solicitado requiere un técnico asignado.");

    private static void EnsureAssignedTechnician(
        Guid changedByUserId,
        Guid technicianUserId)
    {
        if (changedByUserId != technicianUserId)
        {
            throw new DomainException(
                "ONLY_ASSIGNED_TECHNICIAN",
                "Solo el técnico asignado puede iniciar o resolver el ticket.");
        }
    }

    private void EndCurrentAssignment(DateTimeOffset now)
    {
        var current = _assignments.LastOrDefault(assignment => assignment.IsCurrent)
            ?? throw new DomainException("CURRENT_ASSIGNMENT_NOT_FOUND", "No existe una asignación activa.");
        current.End(now);
    }

    private void EndCurrentAssignmentIfAny(DateTimeOffset now)
    {
        var current = _assignments.LastOrDefault(assignment => assignment.IsCurrent);
        current?.End(now);
    }

    private void EndAndClearAssignment(DateTimeOffset now)
    {
        EndCurrentAssignmentIfAny(now);
        CurrentTechnicianUserId = null;
    }

    private void EnsureCreator(Guid userId)
    {
        if (CreatorUserId != userId)
        {
            throw new DomainException("ONLY_TICKET_CREATOR", "La operación solo puede realizarla el creador del ticket.");
        }
    }

    private void EnsureUsable()
    {
        if (IsDeleted)
        {
            throw new DomainException("TICKET_DELETED", "El ticket fue eliminado.");
        }
    }

    private static string NormalizeSubject(string subject) =>
        Guard.Required(subject, SubjectMaxLength, "INVALID_TICKET_SUBJECT", "El asunto es obligatorio y admite máximo 200 caracteres.");

    private static string NormalizeDescription(string description) =>
        Guard.Required(description, DescriptionMaxLength, "INVALID_TICKET_DESCRIPTION", "La descripción es obligatoria y admite máximo 4000 caracteres.");

    private static string NormalizeJustification(string justification, string code) =>
        Guard.Required(justification, JustificationMaxLength, code, "La justificación es obligatoria y admite máximo 1000 caracteres.");
}

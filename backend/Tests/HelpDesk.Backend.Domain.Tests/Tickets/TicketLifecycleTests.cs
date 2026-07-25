using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.ValueObjects;

namespace HelpDesk.Backend.Domain.Tests.Tickets;

public sealed class TicketLifecycleTests
{
    [Fact]
    public void Create_StartsOpenWithInitialSlaAndBusinessNumber()
    {
        var creatorId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var ticket = TestData.Ticket(creatorId, categoryId);

        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Equal("HD-2026-000001", ticket.Number.Value);
        Assert.Equal(TestData.Now.AddHours(2), ticket.CurrentSlaCycle.DeadlineAtUtc);
        Assert.Equal(SlaOutcome.Pending, ticket.CurrentSlaCycle.Outcome);
        Assert.Single(ticket.StatusHistory);
    }

    [Fact]
    public void Assign_AutomaticallyChangesStatusAndCreatesHistory()
    {
        var ticket = TestData.Ticket(Guid.NewGuid(), Guid.NewGuid());
        var technicianId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();

        ticket.Assign(technicianId, supervisorId, TestData.Now.AddMinutes(5));

        Assert.Equal(TicketStatus.Assigned, ticket.Status);
        Assert.Equal(technicianId, ticket.CurrentTechnicianUserId);
        Assert.Single(ticket.Assignments);
        Assert.True(ticket.CountsTowardTechnicianCapacity);
    }

    [Fact]
    public void Reassign_PreservesSlaDeadlineAndRecordsReason()
    {
        var ticket = TestData.Ticket(Guid.NewGuid(), Guid.NewGuid());
        var firstTechnicianId = Guid.NewGuid();
        var secondTechnicianId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        ticket.Assign(firstTechnicianId, supervisorId, TestData.Now.AddMinutes(5));
        var originalDeadline = ticket.CurrentSlaCycle.DeadlineAtUtc;

        ticket.Reassign(
            secondTechnicianId,
            supervisorId,
            "El primer técnico terminó su turno.",
            TestData.Now.AddMinutes(30));

        Assert.Equal(originalDeadline, ticket.CurrentSlaCycle.DeadlineAtUtc);
        Assert.Single(ticket.SlaCycles);
        Assert.Equal(2, ticket.Assignments.Count);
        Assert.False(ticket.Assignments.First().IsCurrent);
        Assert.True(ticket.Assignments.Last().IsCurrent);
        Assert.Equal(TicketCommentType.ReassignmentReason, ticket.Comments.Single().Type);
    }

    [Fact]
    public void TechnicianCanMoveAssignedToProgressAndResolve()
    {
        var ticket = TestData.Ticket(Guid.NewGuid(), Guid.NewGuid());
        var technicianId = Guid.NewGuid();
        ticket.Assign(technicianId, Guid.NewGuid(), TestData.Now.AddMinutes(1));

        ticket.StartProgress(technicianId, TestData.Now.AddMinutes(30));
        ticket.Resolve(technicianId, "Se reemplazó el cable de red.", TestData.Now.AddHours(1));

        Assert.Equal(TicketStatus.Resolved, ticket.Status);
        Assert.Equal(SlaOutcome.Met, ticket.CurrentSlaCycle.Outcome);
        Assert.Equal(technicianId, ticket.CurrentSlaCycle.ResponsibleTechnicianUserId);
        Assert.Contains(ticket.Comments, comment =>
            comment.Type == TicketCommentType.Resolution && comment.SatisfiesResolutionRequirement);
    }

    [Fact]
    public void ReopenedTicket_CanBeResolvedDirectly()
    {
        var creatorId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();
        var ticket = CreateResolvedTicket(creatorId, technicianId);

        ticket.ReopenByCreator(creatorId, true, TimeSpan.FromHours(2), TestData.Now.AddHours(2));
        ticket.Resolve(technicianId, "Se aplicó un ajuste adicional.", TestData.Now.AddHours(2.5));

        Assert.Equal(TicketStatus.Resolved, ticket.Status);
        Assert.Equal(2, ticket.SlaCycles.Count);
        Assert.Equal(SlaOutcome.Met, ticket.CurrentSlaCycle.Outcome);
    }

    [Fact]
    public void ReopenWithoutTechnicianCapacity_ClearsAssignment()
    {
        var creatorId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();
        var ticket = CreateResolvedTicket(creatorId, technicianId);

        ticket.ReopenByCreator(creatorId, false, TimeSpan.FromHours(2), TestData.Now.AddHours(2));

        Assert.Equal(TicketStatus.Reopened, ticket.Status);
        Assert.Null(ticket.CurrentTechnicianUserId);
        Assert.False(ticket.CountsTowardTechnicianCapacity);
        Assert.False(ticket.Assignments.Last().IsCurrent);
    }

    [Fact]
    public void AutomaticClose_RequiresFortyEightHourWindowToExpire()
    {
        var creatorId = Guid.NewGuid();
        var ticket = CreateResolvedTicket(creatorId, Guid.NewGuid());
        var resolvedAt = ticket.ResolvedAtUtc!.Value;

        Assert.Throws<DomainException>(() => ticket.CloseAutomatically(resolvedAt.AddHours(47)));

        ticket.CloseAutomatically(resolvedAt.AddHours(48));

        Assert.Equal(TicketStatus.Closed, ticket.Status);
        Assert.NotNull(ticket.ClosedAtUtc);
    }

    [Fact]
    public void DeleteByCreator_IsAllowedOnlyBeforeFirstAssignment()
    {
        var creatorId = Guid.NewGuid();
        var ticket = TestData.Ticket(creatorId, Guid.NewGuid());

        ticket.DeleteByCreator(creatorId, TestData.Now.AddMinutes(1));

        Assert.True(ticket.IsDeleted);

        var assignedTicket = TestData.Ticket(creatorId, Guid.NewGuid());
        assignedTicket.Assign(Guid.NewGuid(), Guid.NewGuid(), TestData.Now.AddMinutes(1));
        Assert.Throws<DomainException>(() =>
            assignedTicket.DeleteByCreator(creatorId, TestData.Now.AddMinutes(2)));
    }

    [Fact]
    public void ForceClose_RequiresJustificationAndRecordsAdministrativeEvidence()
    {
        var ticket = TestData.Ticket(Guid.NewGuid(), Guid.NewGuid());
        var adminId = Guid.NewGuid();

        Assert.Throws<DomainException>(() =>
            ticket.ForceTransition(TicketStatus.Closed, adminId, " ", TestData.Now.AddMinutes(1)));

        ticket.ForceTransition(
            TicketStatus.Closed,
            adminId,
            "Solicitud duplicada confirmada.",
            TestData.Now.AddMinutes(2));

        Assert.Equal(TicketStatus.Closed, ticket.Status);
        Assert.Contains(ticket.Comments, comment =>
            comment.Type == TicketCommentType.AdministrativeJustification &&
            comment.SatisfiesResolutionRequirement);
    }

    [Fact]
    public void HistoryCollections_AreReadOnly()
    {
        var ticket = TestData.Ticket(Guid.NewGuid(), Guid.NewGuid());
        var collection = Assert.IsAssignableFrom<ICollection<HelpDesk.Backend.Domain.Aggregates.Tickets.TicketStatusChange>>(
            ticket.StatusHistory);

        Assert.True(collection.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => collection.Clear());
    }

    private static HelpDesk.Backend.Domain.Aggregates.Tickets.Ticket CreateResolvedTicket(
        Guid creatorId,
        Guid technicianId)
    {
        var ticket = TestData.Ticket(creatorId, Guid.NewGuid());
        ticket.Assign(technicianId, Guid.NewGuid(), TestData.Now.AddMinutes(1));
        ticket.StartProgress(technicianId, TestData.Now.AddMinutes(10));
        ticket.Resolve(technicianId, "Incidente solucionado.", TestData.Now.AddHours(1));
        return ticket;
    }
}

using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Domain.Tests.Tickets;

public sealed class TicketSlaTests
{
    [Fact]
    public void EvaluateSla_AttributesBreachToTechnicianAssignedAtDeadline()
    {
        var ticket = TestData.Ticket(Guid.NewGuid(), Guid.NewGuid());
        var firstTechnicianId = Guid.NewGuid();
        var secondTechnicianId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        ticket.Assign(firstTechnicianId, supervisorId, TestData.Now.AddMinutes(5));

        ticket.Reassign(
            secondTechnicianId,
            supervisorId,
            "Reasignación posterior al vencimiento.",
            TestData.Now.AddHours(2.5));
        ticket.EvaluateSla(TestData.Now.AddHours(2.6));

        Assert.Equal(SlaOutcome.Breached, ticket.CurrentSlaCycle.Outcome);
        Assert.Equal(firstTechnicianId, ticket.CurrentSlaCycle.ResponsibleTechnicianUserId);
        Assert.Equal(TestData.Now.AddHours(2), ticket.CurrentSlaCycle.DeadlineAtUtc);
    }

    [Fact]
    public void EvaluateSla_LeavesTechnicianEmptyWhenTicketWasUnassignedAtDeadline()
    {
        var ticket = TestData.Ticket(Guid.NewGuid(), Guid.NewGuid());

        ticket.EvaluateSla(TestData.Now.AddHours(3));

        Assert.Equal(SlaOutcome.Breached, ticket.CurrentSlaCycle.Outcome);
        Assert.Null(ticket.CurrentSlaCycle.ResponsibleTechnicianUserId);
    }

    [Fact]
    public void ResponseAfterDeadline_UsesTechnicianAssignedAtDeadline()
    {
        var ticket = TestData.Ticket(Guid.NewGuid(), Guid.NewGuid());
        var firstTechnicianId = Guid.NewGuid();
        var secondTechnicianId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        ticket.Assign(firstTechnicianId, supervisorId, TestData.Now.AddMinutes(1));
        ticket.Reassign(
            secondTechnicianId,
            supervisorId,
            "Cambio de turno después del límite.",
            TestData.Now.AddHours(2.25));

        ticket.StartProgress(secondTechnicianId, TestData.Now.AddHours(2.5));

        Assert.Equal(SlaOutcome.Breached, ticket.CurrentSlaCycle.Outcome);
        Assert.Equal(firstTechnicianId, ticket.CurrentSlaCycle.ResponsibleTechnicianUserId);
        Assert.Equal(TestData.Now.AddHours(2.5), ticket.CurrentSlaCycle.RespondedAtUtc);
    }

    [Fact]
    public void Reopening_CreatesIndependentSlaCycle()
    {
        var creatorId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();
        var ticket = TestData.Ticket(creatorId, Guid.NewGuid());
        ticket.Assign(technicianId, Guid.NewGuid(), TestData.Now.AddMinutes(1));
        ticket.StartProgress(technicianId, TestData.Now.AddMinutes(10));
        ticket.Resolve(technicianId, "Solucionado.", TestData.Now.AddMinutes(30));

        ticket.ReopenByCreator(
            creatorId,
            true,
            TimeSpan.FromHours(4),
            TestData.Now.AddHours(1));

        Assert.Equal(2, ticket.SlaCycles.Count);
        Assert.Equal(SlaOutcome.Met, ticket.SlaCycles.First().Outcome);
        Assert.Equal(SlaOutcome.Pending, ticket.CurrentSlaCycle.Outcome);
        Assert.Equal(TestData.Now.AddHours(5), ticket.CurrentSlaCycle.DeadlineAtUtc);
    }
}

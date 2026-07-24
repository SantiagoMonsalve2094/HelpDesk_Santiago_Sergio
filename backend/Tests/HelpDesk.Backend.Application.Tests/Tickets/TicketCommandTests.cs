using HelpDesk.Backend.Application.Features.Tickets.Commands.AddTicketComment;
using HelpDesk.Backend.Application.Features.Tickets.Commands.AssignTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.CloseResolvedTickets;
using HelpDesk.Backend.Application.Features.Tickets.Commands.CreateTicket;
using HelpDesk.Backend.Application.Features.Sla.Commands.EvaluatePendingSla;
using HelpDesk.Backend.Application.Features.Tickets.Commands.ForceTicketStatus;
using HelpDesk.Backend.Application.Features.Tickets.Commands.ReassignTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.ReopenTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.ResolveTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.StartTicketProgress;
using HelpDesk.Backend.Application.Tests.TestDoubles;
using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Application.Tests.Tickets;

public sealed class TicketCommandTests
{
    [Fact]
    public async Task CreateTicket_UsesAnnualSequenceAndCategorySla()
    {
        var context = new TestContext();
        var creator = ApplicationTestData.User();
        var category = ApplicationTestData.Category();
        context.Add(creator);
        context.Add(category);
        context.Sequence.Next = 42;
        var handler = new CreateTicketHandler(
            context.UnitOfWork,
            context.Clock,
            new CreateTicketValidator());
        using var cancellation = new CancellationTokenSource();

        var response = await handler.Handle(
            new CreateTicketCommand(
                creator.Id,
                "Falla de software",
                "La aplicación no inicia.",
                category.Id,
                TicketPriority.High),
            cancellation.Token);

        Assert.Equal("HD-2026-000042", response.TicketNumber);
        Assert.Equal(TimeSpan.FromHours(6), context.Tickets.AddedTicket!.CurrentSlaCycle.Duration);
        Assert.Equal(2026, context.Sequence.ReceivedYear);
        Assert.Equal(cancellation.Token, context.Sequence.ReceivedCancellationToken);
        Assert.Equal(1, context.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task AssignTicket_ValidTechnician_ChangesStatusAndPersists()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var creator = ApplicationTestData.User();
        var supervisor = ApplicationTestData.Supervisor(category.Id);
        var technician = ApplicationTestData.Technician([category.Id], 2);
        var ticket = ApplicationTestData.Ticket(creator.Id, category.Id);
        context.Add(category);
        context.Add(creator, supervisor, technician);
        context.Add(ticket);
        var handler = new AssignTicketHandler(
            context.UnitOfWork,
            context.Clock,
            new AssignTicketValidator());

        await handler.Handle(
            new AssignTicketCommand(supervisor.Id, ticket.Id, technician.Id),
            CancellationToken.None);

        Assert.Equal(TicketStatus.Assigned, ticket.Status);
        Assert.Equal(technician.Id, ticket.CurrentTechnicianUserId);
        Assert.Single(ticket.Assignments);
        Assert.Equal(1, context.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task AssignTicket_TechnicianAtCapacity_IsRejected()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var creator = ApplicationTestData.User();
        var admin = ApplicationTestData.SuperAdmin();
        var technician = ApplicationTestData.Technician([category.Id], 2);
        var ticket = ApplicationTestData.Ticket(creator.Id, category.Id);
        context.Add(category);
        context.Add(creator, admin, technician);
        context.Add(ticket);
        context.Tickets.ActiveCountByTechnician[technician.Id] = 2;
        var handler = new AssignTicketHandler(
            context.UnitOfWork,
            context.Clock,
            new AssignTicketValidator());

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(
                new AssignTicketCommand(admin.Id, ticket.Id, technician.Id),
                CancellationToken.None));

        Assert.Equal("TECHNICIAN_AT_CAPACITY", exception.Code);
        Assert.Equal(0, context.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task AssignTicket_SupervisorOfAnotherCategory_IsForbidden()
    {
        var context = new TestContext();
        var ticketCategory = ApplicationTestData.Category();
        var otherCategory = ApplicationTestData.Category();
        var creator = ApplicationTestData.User();
        var supervisor = ApplicationTestData.Supervisor(otherCategory.Id);
        var technician = ApplicationTestData.Technician([ticketCategory.Id]);
        var ticket = ApplicationTestData.Ticket(creator.Id, ticketCategory.Id);
        context.Add(ticketCategory, otherCategory);
        context.Add(creator, supervisor, technician);
        context.Add(ticket);
        var handler = new AssignTicketHandler(
            context.UnitOfWork,
            context.Clock,
            new AssignTicketValidator());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new AssignTicketCommand(supervisor.Id, ticket.Id, technician.Id),
                CancellationToken.None));
    }

    [Fact]
    public async Task ReassignTicket_PreservesSlaDeadlineAndAssignmentHistory()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var creator = ApplicationTestData.User();
        var admin = ApplicationTestData.SuperAdmin();
        var firstTechnician = ApplicationTestData.Technician([category.Id]);
        var secondTechnician = ApplicationTestData.Technician([category.Id]);
        var ticket = ApplicationTestData.Ticket(creator.Id, category.Id);
        ticket.Assign(firstTechnician.Id, admin.Id, ApplicationTestData.Now);
        var deadline = ticket.CurrentSlaCycle.DeadlineAtUtc;
        var cycleId = ticket.CurrentSlaCycle.Id;
        context.Add(category);
        context.Add(creator, admin, firstTechnician, secondTechnician);
        context.Add(ticket);
        context.Clock.UtcNow = ApplicationTestData.Now.AddMinutes(30);
        var handler = new ReassignTicketHandler(
            context.UnitOfWork,
            context.Clock,
            new ReassignTicketValidator());

        await handler.Handle(
            new ReassignTicketCommand(
                admin.Id,
                ticket.Id,
                secondTechnician.Id,
                "Cambio de turno"),
            CancellationToken.None);

        Assert.Equal(cycleId, ticket.CurrentSlaCycle.Id);
        Assert.Equal(deadline, ticket.CurrentSlaCycle.DeadlineAtUtc);
        Assert.Equal(2, ticket.Assignments.Count);
        Assert.Equal("Cambio de turno", ticket.Assignments.Last().Reason);
        Assert.Equal(secondTechnician.Id, ticket.CurrentTechnicianUserId);
    }

    [Fact]
    public async Task StartProgress_ByUnassignedTechnician_IsForbidden()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var creator = ApplicationTestData.User();
        var admin = ApplicationTestData.SuperAdmin();
        var assigned = ApplicationTestData.Technician([category.Id]);
        var other = ApplicationTestData.Technician([category.Id]);
        var ticket = ApplicationTestData.Ticket(creator.Id, category.Id);
        ticket.Assign(assigned.Id, admin.Id, ApplicationTestData.Now);
        context.Add(category);
        context.Add(creator, admin, assigned, other);
        context.Add(ticket);
        var handler = new StartTicketProgressHandler(
            context.UnitOfWork,
            context.Clock,
            new StartTicketProgressValidator());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new StartTicketProgressCommand(other.Id, ticket.Id),
                CancellationToken.None));
    }

    [Fact]
    public async Task ResolveTicket_ByCategorySupervisor_AddsResolutionComment()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var creator = ApplicationTestData.User();
        var supervisor = ApplicationTestData.Supervisor(category.Id);
        var technician = ApplicationTestData.Technician([category.Id]);
        var ticket = ApplicationTestData.Ticket(creator.Id, category.Id);
        ticket.Assign(technician.Id, supervisor.Id, ApplicationTestData.Now);
        ticket.StartProgress(technician.Id, ApplicationTestData.Now.AddMinutes(10));
        context.Add(category);
        context.Add(creator, supervisor, technician);
        context.Add(ticket);
        context.Clock.UtcNow = ApplicationTestData.Now.AddMinutes(20);
        var handler = new ResolveTicketHandler(
            context.UnitOfWork,
            context.Clock,
            new ResolveTicketValidator());

        await handler.Handle(
            new ResolveTicketCommand(supervisor.Id, ticket.Id, "Servicio restaurado."),
            CancellationToken.None);

        Assert.Equal(TicketStatus.Resolved, ticket.Status);
        Assert.Contains(ticket.Comments, comment =>
            comment.Type == TicketCommentType.Resolution &&
            comment.Body == "Servicio restaurado.");
    }

    [Fact]
    public async Task AddComment_ByCreator_IsAllowed()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var creator = ApplicationTestData.User();
        var ticket = ApplicationTestData.Ticket(creator.Id, category.Id);
        context.Add(category);
        context.Add(creator);
        context.Add(ticket);
        var handler = new AddTicketCommentHandler(
            context.UnitOfWork,
            context.Clock,
            new AddTicketCommentValidator());

        await handler.Handle(
            new AddTicketCommentCommand(creator.Id, ticket.Id, "Información adicional."),
            CancellationToken.None);

        Assert.Contains(ticket.Comments, comment =>
            comment.Type == TicketCommentType.General);
    }

    [Fact]
    public async Task ReopenTicket_WithPreviousTechnicianCapacity_RetainsAssignmentAndCreatesCycle()
    {
        var context = CreateResolvedTicketContext(out var creator, out var technician, out var ticket);
        var previousCycleId = ticket.CurrentSlaCycle.Id;
        context.Clock.UtcNow = ticket.ResolvedAtUtc!.Value.AddHours(1);
        context.Tickets.ActiveCountByTechnician[technician.Id] = 1;
        var handler = new ReopenTicketHandler(
            context.UnitOfWork,
            context.Clock,
            new ReopenTicketValidator());

        await handler.Handle(
            new ReopenTicketCommand(creator.Id, ticket.Id),
            CancellationToken.None);

        Assert.Equal(TicketStatus.Reopened, ticket.Status);
        Assert.Equal(technician.Id, ticket.CurrentTechnicianUserId);
        Assert.NotEqual(previousCycleId, ticket.CurrentSlaCycle.Id);
        Assert.Equal(SlaCycleTrigger.Reopening, ticket.CurrentSlaCycle.Trigger);
    }

    [Fact]
    public async Task ReopenTicket_WithoutPreviousTechnicianCapacity_ClearsAssignment()
    {
        var context = CreateResolvedTicketContext(out var creator, out var technician, out var ticket);
        context.Clock.UtcNow = ticket.ResolvedAtUtc!.Value.AddHours(1);
        context.Tickets.ActiveCountByTechnician[technician.Id] =
            technician.TechnicianProfile!.MaxActiveTickets;
        var handler = new ReopenTicketHandler(
            context.UnitOfWork,
            context.Clock,
            new ReopenTicketValidator());

        await handler.Handle(
            new ReopenTicketCommand(creator.Id, ticket.Id),
            CancellationToken.None);

        Assert.Equal(TicketStatus.Reopened, ticket.Status);
        Assert.Null(ticket.CurrentTechnicianUserId);
        Assert.False(ticket.Assignments.Last().IsCurrent);
    }

    [Fact]
    public async Task ForceStatus_BySupervisorOfAnotherCategory_IsForbidden()
    {
        var context = new TestContext();
        var ticketCategory = ApplicationTestData.Category();
        var otherCategory = ApplicationTestData.Category();
        var creator = ApplicationTestData.User();
        var supervisor = ApplicationTestData.Supervisor(otherCategory.Id);
        var ticket = ApplicationTestData.Ticket(creator.Id, ticketCategory.Id);
        context.Add(ticketCategory, otherCategory);
        context.Add(creator, supervisor);
        context.Add(ticket);
        var handler = new ForceTicketStatusHandler(
            context.UnitOfWork,
            context.Clock,
            new ForceTicketStatusValidator());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new ForceTicketStatusCommand(
                    supervisor.Id,
                    ticket.Id,
                    TicketStatus.Closed,
                    "Cierre administrativo"),
                CancellationToken.None));
    }

    [Fact]
    public async Task EvaluatePendingSla_BreachesAndAttributesTechnicianAtDeadline()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var creator = ApplicationTestData.User();
        var admin = ApplicationTestData.SuperAdmin();
        var technician = ApplicationTestData.Technician([category.Id]);
        var ticket = ApplicationTestData.Ticket(creator.Id, category.Id);
        ticket.Assign(technician.Id, admin.Id, ApplicationTestData.Now);
        context.Tickets.PendingSlaTickets.Add(ticket);
        context.Clock.UtcNow = ticket.CurrentSlaCycle.DeadlineAtUtc.AddMinutes(1);
        var handler = new EvaluatePendingSlaHandler(
            context.UnitOfWork,
            context.Clock,
            new EvaluatePendingSlaValidator());

        var count = await handler.Handle(
            new EvaluatePendingSlaCommand(),
            CancellationToken.None);

        Assert.Equal(1, count);
        Assert.Equal(SlaOutcome.Breached, ticket.CurrentSlaCycle.Outcome);
        Assert.Equal(technician.Id, ticket.CurrentSlaCycle.ResponsibleTechnicianUserId);
        Assert.Equal(1, context.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task EvaluatePendingSla_WithoutTechnicianLeavesIndividualAttributionEmpty()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var creator = ApplicationTestData.User();
        var ticket = ApplicationTestData.Ticket(creator.Id, category.Id);
        context.Tickets.PendingSlaTickets.Add(ticket);
        context.Clock.UtcNow = ticket.CurrentSlaCycle.DeadlineAtUtc.AddMinutes(1);
        var handler = new EvaluatePendingSlaHandler(
            context.UnitOfWork,
            context.Clock,
            new EvaluatePendingSlaValidator());

        await handler.Handle(new EvaluatePendingSlaCommand(), CancellationToken.None);

        Assert.Equal(SlaOutcome.Breached, ticket.CurrentSlaCycle.Outcome);
        Assert.Null(ticket.CurrentSlaCycle.ResponsibleTechnicianUserId);
        Assert.Equal(category.Id, ticket.CurrentSlaCycle.SupportCategoryId);
    }

    [Fact]
    public async Task CloseResolvedTickets_AfterFortyEightHours_ClosesAutomatically()
    {
        var context = CreateResolvedTicketContext(out _, out _, out var ticket);
        context.Clock.UtcNow = ticket.ResolvedAtUtc!.Value.AddHours(48);
        context.Tickets.ResolvedForClosure.Add(ticket);
        var handler = new CloseResolvedTicketsHandler(
            context.UnitOfWork,
            context.Clock,
            new CloseResolvedTicketsValidator());

        var count = await handler.Handle(
            new CloseResolvedTicketsCommand(),
            CancellationToken.None);

        Assert.Equal(1, count);
        Assert.Equal(TicketStatus.Closed, ticket.Status);
        Assert.True(ticket.StatusHistory.Last().IsAutomatic);
    }

    private static TestContext CreateResolvedTicketContext(
        out HelpDesk.Backend.Domain.Aggregates.Users.User creator,
        out HelpDesk.Backend.Domain.Aggregates.Users.User technician,
        out HelpDesk.Backend.Domain.Aggregates.Tickets.Ticket ticket)
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        creator = ApplicationTestData.User();
        var admin = ApplicationTestData.SuperAdmin();
        technician = ApplicationTestData.Technician([category.Id], 3);
        ticket = ApplicationTestData.Ticket(creator.Id, category.Id);
        ticket.Assign(technician.Id, admin.Id, ApplicationTestData.Now);
        ticket.StartProgress(technician.Id, ApplicationTestData.Now.AddMinutes(10));
        ticket.Resolve(
            technician.Id,
            "Problema solucionado.",
            ApplicationTestData.Now.AddMinutes(20));
        context.Add(category);
        context.Add(creator, admin, technician);
        context.Add(ticket);
        return context;
    }
}

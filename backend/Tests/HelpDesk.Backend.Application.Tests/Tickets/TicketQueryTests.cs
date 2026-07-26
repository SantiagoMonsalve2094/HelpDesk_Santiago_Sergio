using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using HelpDesk.Backend.Application.Features.Tickets.Queries.GetAssignableTechnicians;
using HelpDesk.Backend.Application.Features.Sla.Queries.GetSlaAlerts;
using HelpDesk.Backend.Application.Features.Sla.Queries.GetSlaReport;
using HelpDesk.Backend.Application.Features.Tickets.Queries.GetTicketById;
using HelpDesk.Backend.Application.Features.Tickets.Queries.GetTickets;
using HelpDesk.Backend.Application.Tests.TestDoubles;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Application.Tests.Tickets;

public sealed class TicketQueryTests
{
    [Fact]
    public async Task GetTicketById_ReturnsAllHistoriesForVisibleActor()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var creator = ApplicationTestData.User();
        var admin = ApplicationTestData.SuperAdmin();
        var technician = ApplicationTestData.Technician([category.Id]);
        var ticket = ApplicationTestData.Ticket(creator.Id, category.Id);
        ticket.Assign(technician.Id, admin.Id, ApplicationTestData.Now);
        ticket.AddGeneralComment(creator.Id, "Comentario", ApplicationTestData.Now);
        context.Add(category);
        context.Add(creator, admin, technician);
        context.Add(ticket);
        var handler = new GetTicketByIdHandler(
            context.UnitOfWork,
            new GetTicketByIdValidator());

        var response = await handler.Handle(
            new GetTicketByIdQuery(creator.Id, ticket.Id),
            CancellationToken.None);

        Assert.Single(response.Assignments);
        Assert.Single(response.Comments);
        Assert.Equal(2, response.StatusHistory.Count);
        Assert.Single(response.SlaCycles);
    }

    [Fact]
    public async Task GetTicketById_UnrelatedUser_IsForbidden()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var creator = ApplicationTestData.User();
        var unrelated = ApplicationTestData.User();
        var ticket = ApplicationTestData.Ticket(creator.Id, category.Id);
        context.Add(category);
        context.Add(creator, unrelated);
        context.Add(ticket);
        var handler = new GetTicketByIdHandler(
            context.UnitOfWork,
            new GetTicketByIdValidator());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new GetTicketByIdQuery(unrelated.Id, ticket.Id),
                CancellationToken.None));
    }

    [Fact]
    public async Task GetTickets_CreatesSupervisorCategoryScopeAndForwardsFilters()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var supervisor = ApplicationTestData.Supervisor(category.Id);
        context.Add(supervisor);
        var readRepository = new FakeTicketReadRepository();
        var handler = new GetTicketsHandler(
            context.UnitOfWork,
            readRepository,
            new GetTicketsValidator());
        using var cancellation = new CancellationTokenSource();

        await handler.Handle(
            new GetTicketsQuery(
                supervisor.Id,
                Status: TicketStatus.Assigned,
                IsOverdue: true),
            cancellation.Token);

        Assert.Equal(UserRole.Supervisor, readRepository.ReceivedTicketFilter!.Visibility.ActorRole);
        Assert.Equal(category.Id, readRepository.ReceivedTicketFilter.Visibility.SupervisorSupportCategoryId);
        Assert.Equal(TicketStatus.Assigned, readRepository.ReceivedTicketFilter.Status);
        Assert.True(readRepository.ReceivedTicketFilter.IsOverdue);
        Assert.Equal(cancellation.Token, readRepository.ReceivedCancellationToken);
    }

    [Fact]
    public async Task GetTickets_ForTechnicianCarriesActorForCreatedOrAssignedVisibility()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var technician = ApplicationTestData.Technician([category.Id]);
        context.Add(technician);
        var readRepository = new FakeTicketReadRepository();
        var handler = new GetTicketsHandler(
            context.UnitOfWork,
            readRepository,
            new GetTicketsValidator());

        await handler.Handle(
            new GetTicketsQuery(technician.Id),
            CancellationToken.None);

        Assert.Equal(technician.Id, readRepository.ReceivedTicketFilter!.Visibility.ActorUserId);
        Assert.Equal(UserRole.Technician, readRepository.ReceivedTicketFilter.Visibility.ActorRole);
    }

    [Fact]
    public async Task GetAssignableTechnicians_RequiresAssignmentPermission()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var creator = ApplicationTestData.User();
        var ticket = ApplicationTestData.Ticket(creator.Id, category.Id);
        context.Add(creator);
        context.Add(ticket);
        var handler = new GetAssignableTechniciansHandler(
            context.UnitOfWork,
            new FakeTicketReadRepository(),
            new GetAssignableTechniciansValidator());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new GetAssignableTechniciansQuery(creator.Id, ticket.Id),
                CancellationToken.None));
    }

    [Fact]
    public async Task GetSlaAlerts_UsesSameVisibilityScope()
    {
        var context = new TestContext();
        var user = ApplicationTestData.User();
        context.Add(user);
        var readRepository = new FakeTicketReadRepository();
        var handler = new GetSlaAlertsHandler(
            context.UnitOfWork,
            readRepository,
            context.Clock,
            new GetSlaAlertsValidator());

        await handler.Handle(
            new GetSlaAlertsQuery(user.Id),
            CancellationToken.None);

        Assert.Equal(user.Id, readRepository.ReceivedVisibility!.ActorUserId);
        Assert.Equal(UserRole.User, readRepository.ReceivedVisibility.ActorRole);
        Assert.Equal(context.Clock.UtcNow, readRepository.ReceivedAsOfUtc);
    }

    [Fact]
<<<<<<< HEAD
    public async Task GetSlaReport_SupervisorCanFilterAnyCategory()
=======
    public async Task GetSlaReport_SupervisorIsRestrictedToOwnCategory()
>>>>>>> 60bd3aa8c163527f2e018e15a29114b99aa06847
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var otherCategory = ApplicationTestData.Category();
        var supervisor = ApplicationTestData.Supervisor(category.Id);
        context.Add(supervisor);
        var handler = new GetSlaReportHandler(
            context.UnitOfWork,
            new FakeSlaReportReadRepository(),
            new GetSlaReportValidator());

<<<<<<< HEAD
        await handler.Handle(
            new GetSlaReportQuery(
                supervisor.Id,
                SupportCategoryId: otherCategory.Id),
            CancellationToken.None);
    }

    [Fact]
    public async Task GetSlaReport_SupervisorWithoutFilterReceivesAllCategories()
=======
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new GetSlaReportQuery(
                    supervisor.Id,
                    SupportCategoryId: otherCategory.Id),
                CancellationToken.None));
    }

    [Fact]
    public async Task GetSlaReport_SupervisorFilterIsForcedToOwnCategory()
>>>>>>> 60bd3aa8c163527f2e018e15a29114b99aa06847
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var supervisor = ApplicationTestData.Supervisor(category.Id);
        context.Add(supervisor);
        var readRepository = new FakeSlaReportReadRepository
        {
            Response = new SlaReportResponse(
                [
                    new SlaComplianceGroupResponse(
                        category.Id,
                        category.Name,
                        null,
                        SlaReportLabels.UnassignedTechnician,
                        0,
                        1,
                        1,
                        1,
                        0)
                ],
                0,
                1,
                1,
                1,
                0)
        };
        var handler = new GetSlaReportHandler(
            context.UnitOfWork,
            readRepository,
            new GetSlaReportValidator());

        var response = await handler.Handle(
            new GetSlaReportQuery(supervisor.Id),
            CancellationToken.None);

<<<<<<< HEAD
        Assert.Null(readRepository.ReceivedFilter!.SupportCategoryId);
=======
        Assert.Equal(category.Id, readRepository.ReceivedFilter!.SupportCategoryId);
>>>>>>> 60bd3aa8c163527f2e018e15a29114b99aa06847
        Assert.Equal(SlaReportLabels.UnassignedTechnician, response.Groups.Single().TechnicianName);
        Assert.Equal(1, response.TotalPendingCycles);
        Assert.Equal(1, response.TotalEvaluatedCycles);
    }

    [Fact]
    public async Task GetSlaReport_NormalUserIsForbidden()
    {
        var context = new TestContext();
        var user = ApplicationTestData.User();
        context.Add(user);
        var handler = new GetSlaReportHandler(
            context.UnitOfWork,
            new FakeSlaReportReadRepository(),
            new GetSlaReportValidator());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new GetSlaReportQuery(user.Id),
                CancellationToken.None));
    }

    [Fact]
    public async Task GetTicketsValidator_RejectsInvalidDateRange()
    {
        var validator = new GetTicketsValidator();

        var result = await validator.ValidateAsync(
            new GetTicketsQuery(
                Guid.NewGuid(),
                CreatedFromUtc: ApplicationTestData.Now,
                CreatedToUtc: ApplicationTestData.Now.AddDays(-1)));

        Assert.False(result.IsValid);
    }
}

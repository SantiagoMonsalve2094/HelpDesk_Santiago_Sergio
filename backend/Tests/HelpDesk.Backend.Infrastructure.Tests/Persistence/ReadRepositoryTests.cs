using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.DTOs.SupportCategories;
using HelpDesk.Backend.Application.DTOs.Users;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Users;
using HelpDesk.Backend.Infrastructure.Tests.Common;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Backend.Infrastructure.Tests;

public sealed class ReadRepositoryTests
{
    [Fact]
    public async Task UserAndCategoryListings_ApplyRoleCategoryAndActiveFilters()
    {
        await using var database = await InfrastructureTestDatabase.CreateAsync();
        var activeCategory = InfrastructureTestData.CreateCategory("Mesa de ayuda");
        var inactiveCategory = InfrastructureTestData.CreateCategory("Legado");
        inactiveCategory.Deactivate(InfrastructureTestData.Now.AddMinutes(1));
        var technician = InfrastructureTestData.CreateTechnician(
            "Técnico Mesa",
            "tecnico.mesa@helpdesk.local",
            [activeCategory.Id]);
        var supervisor = User.CreateSupervisor(
            "Supervisor Mesa",
            "supervisor.mesa@helpdesk.local",
            "hash-supervisor",
            activeCategory.Id,
            InfrastructureTestData.Now);

        await using var scope = database.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.SupportCategories.AddAsync(activeCategory, default);
        await unitOfWork.SupportCategories.AddAsync(inactiveCategory, default);
        await unitOfWork.SaveChangesAsync(default);
        await unitOfWork.Users.AddAsync(technician, default);
        await unitOfWork.Users.AddAsync(supervisor, default);
        await unitOfWork.SaveChangesAsync(default);
        var userReadRepository =
            scope.ServiceProvider.GetRequiredService<IUserReadRepository>();
        var categoryReadRepository =
            scope.ServiceProvider.GetRequiredService<ISupportCategoryReadRepository>();

        var technicians = await userReadRepository.GetPagedAsync(
            new UserReadFilter(
                UserRole.Technician,
                activeCategory.Id,
                true),
            1,
            20,
            default);
        var activeCategories = await categoryReadRepository.GetPagedAsync(
            new SupportCategoryReadFilter(false),
            1,
            20,
            default);
        var allCategories = await categoryReadRepository.GetPagedAsync(
            new SupportCategoryReadFilter(true),
            1,
            20,
            default);

        Assert.Equal(technician.Id, Assert.Single(technicians.Items).Id);
        Assert.Equal(activeCategory.Id, Assert.Single(activeCategories.Items).Id);
        Assert.Equal(2, allCategories.TotalCount);
    }

    [Fact]
    public async Task TicketListing_AppliesVisibilityForEveryRole()
    {
        await using var database = await InfrastructureTestDatabase.CreateAsync();
        var hardware = InfrastructureTestData.CreateCategory("Hardware");
        var software = InfrastructureTestData.CreateCategory("Software");
        var firstUser = InfrastructureTestData.CreateUser(
            "Primer Usuario",
            "primer.usuario@helpdesk.local");
        var secondUser = InfrastructureTestData.CreateUser(
            "Segundo Usuario",
            "segundo.usuario@helpdesk.local");
        var technician = InfrastructureTestData.CreateTechnician(
            "Técnico Visible",
            "tecnico.visible@helpdesk.local",
            [hardware.Id, software.Id]);
        var supervisor = User.CreateSupervisor(
            "Supervisor Visible",
            "supervisor.visible@helpdesk.local",
            "hash-supervisor",
            hardware.Id,
            InfrastructureTestData.Now);
        var superAdmin = User.CreateSuperAdmin(
            "Santiago Monsalve",
            "admin@helpdesk.local",
            "hash-admin",
            InfrastructureTestData.Now);
        var firstTicket = InfrastructureTestData.CreateTicket(
            1,
            firstUser,
            hardware);
        firstTicket.Assign(
            technician.Id,
            supervisor.Id,
            InfrastructureTestData.Now.AddMinutes(1));
        var secondTicket = InfrastructureTestData.CreateTicket(
            2,
            secondUser,
            software);
        var technicianTicket = InfrastructureTestData.CreateTicket(
            3,
            technician,
            software);

        await using var scope = database.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.SupportCategories.AddAsync(hardware, default);
        await unitOfWork.SupportCategories.AddAsync(software, default);
        await unitOfWork.SaveChangesAsync(default);
        foreach (var user in new[]
                 {
                     firstUser,
                     secondUser,
                     technician,
                     supervisor,
                     superAdmin
                 })
        {
            await unitOfWork.Users.AddAsync(user, default);
        }

        await unitOfWork.SaveChangesAsync(default);
        foreach (var ticket in new[]
                 {
                     firstTicket,
                     secondTicket,
                     technicianTicket
                 })
        {
            await unitOfWork.Tickets.AddAsync(ticket, default);
        }

        await unitOfWork.SaveChangesAsync(default);
        var readRepository =
            scope.ServiceProvider.GetRequiredService<ITicketReadRepository>();

        var userResult = await GetTicketsAsync(
            readRepository,
            new TicketVisibilityScope(firstUser.Id, UserRole.User, null));
        var technicianResult = await GetTicketsAsync(
            readRepository,
            new TicketVisibilityScope(
                technician.Id,
                UserRole.Technician,
                null));
        var supervisorResult = await GetTicketsAsync(
            readRepository,
            new TicketVisibilityScope(
                supervisor.Id,
                UserRole.Supervisor,
                hardware.Id));
        var adminResult = await GetTicketsAsync(
            readRepository,
            new TicketVisibilityScope(
                superAdmin.Id,
                UserRole.SuperAdmin,
                null));

        Assert.Equal(new[] { firstTicket.Id }, userResult.Items.Select(item => item.Id));
        Assert.Equal(
            new[] { firstTicket.Id },
            technicianResult.Items.Select(item => item.Id));
        Assert.Equal(
            new[] { firstTicket.Id, secondTicket.Id, technicianTicket.Id }.Order(),
            supervisorResult.Items.Select(item => item.Id).Order());
        Assert.Equal(3, adminResult.TotalCount);
    }

    [Fact]
    public async Task AssignableTechnicians_IncludesFullTechniciansWithZeroCapacity()
    {
        await using var database = await InfrastructureTestDatabase.CreateAsync();
        var category = InfrastructureTestData.CreateCategory("Telefonía");
        var creator = InfrastructureTestData.CreateUser(
            "Daniel Torres",
            "telefonia@helpdesk.local");
        var busyTechnician = InfrastructureTestData.CreateTechnician(
            "Técnico Ocupado",
            "ocupado@helpdesk.local",
            [category.Id],
            1);
        var availableTechnician = InfrastructureTestData.CreateTechnician(
            "Técnico Disponible",
            "disponible@helpdesk.local",
            [category.Id],
            2);
        var ticket = InfrastructureTestData.CreateTicket(1, creator, category);
        ticket.Assign(
            busyTechnician.Id,
            availableTechnician.Id,
            InfrastructureTestData.Now.AddMinutes(1));

        await using var scope = database.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.SupportCategories.AddAsync(category, default);
        await unitOfWork.SaveChangesAsync(default);
        foreach (var user in new[] { creator, busyTechnician, availableTechnician })
        {
            await unitOfWork.Users.AddAsync(user, default);
        }

        await unitOfWork.SaveChangesAsync(default);
        await unitOfWork.Tickets.AddAsync(ticket, default);
        await unitOfWork.SaveChangesAsync(default);
        var readRepository =
            scope.ServiceProvider.GetRequiredService<ITicketReadRepository>();

        var technicians =
            await readRepository.GetAssignableTechniciansAsync(category.Id, default);

        Assert.Equal(2, technicians.Count);
        Assert.Contains(technicians, technician =>
            technician.TechnicianUserId == availableTechnician.Id &&
            technician.AvailableCapacity == 2);
        Assert.Contains(technicians, technician =>
            technician.TechnicianUserId == busyTechnician.Id &&
            technician.AvailableCapacity == 0);
    }

    [Fact]
    public async Task SlaAlertsAndReport_UseCycleOutcomeAndPendingAssignmentRules()
    {
        await using var database = await InfrastructureTestDatabase.CreateAsync();
        var category = InfrastructureTestData.CreateCategory("Accesos");
        var creator = InfrastructureTestData.CreateUser(
            "Camila Restrepo",
            "accesos@helpdesk.local");
        var technician = InfrastructureTestData.CreateTechnician(
            "Técnico Accesos",
            "tecnico.accesos@helpdesk.local",
            [category.Id],
            5);
        var superAdmin = User.CreateSuperAdmin(
            "Santiago Monsalve",
            "admin.sla@helpdesk.local",
            "hash-admin",
            InfrastructureTestData.Now);
        var metTicket = InfrastructureTestData.CreateTicket(1, creator, category);
        metTicket.Assign(
            technician.Id,
            superAdmin.Id,
            InfrastructureTestData.Now.AddMinutes(1));
        metTicket.StartProgress(
            technician.Id,
            InfrastructureTestData.Now.AddMinutes(10));
        var breachedTicket = InfrastructureTestData.CreateTicket(
            2,
            creator,
            category);
        breachedTicket.Assign(
            technician.Id,
            superAdmin.Id,
            InfrastructureTestData.Now.AddMinutes(1));
        breachedTicket.EvaluateSla(InfrastructureTestData.Now.AddHours(2));
        var pendingTicket = InfrastructureTestData.CreateTicket(
            3,
            creator,
            category);

        await using var scope = database.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.SupportCategories.AddAsync(category, default);
        await unitOfWork.SaveChangesAsync(default);
        foreach (var user in new[] { creator, technician, superAdmin })
        {
            await unitOfWork.Users.AddAsync(user, default);
        }

        await unitOfWork.SaveChangesAsync(default);
        foreach (var ticket in new[] { metTicket, breachedTicket, pendingTicket })
        {
            await unitOfWork.Tickets.AddAsync(ticket, default);
        }

        await unitOfWork.SaveChangesAsync(default);
        var ticketReadRepository =
            scope.ServiceProvider.GetRequiredService<ITicketReadRepository>();
        var reportRepository =
            scope.ServiceProvider.GetRequiredService<ISlaReportReadRepository>();
        var visibility = new TicketVisibilityScope(
            superAdmin.Id,
            UserRole.SuperAdmin,
            null);

        var alerts = await ticketReadRepository.GetSlaAlertsAsync(
            visibility,
            category.Id,
            InfrastructureTestData.Now.AddHours(2),
            1,
            20,
            default);
        var report = await reportRepository.GetReportAsync(
            new SlaReportFilter(category.Id, null, null, null),
            default);

        Assert.Equal(2, alerts.TotalCount);
        Assert.All(alerts.Items, alert => Assert.True(alert.IsBreached));
        Assert.Equal(1, report.TotalMetCycles);
        Assert.Equal(1, report.TotalBreachedCycles);
        Assert.Equal(1, report.TotalPendingCycles);
        Assert.Equal(2, report.TotalEvaluatedCycles);
        Assert.Equal(50m, report.CompliancePercentage);
        var unassigned = Assert.Single(
            report.Groups.Where(group => group.TechnicianUserId is null));
        Assert.Equal(SlaReportLabels.UnassignedTechnician, unassigned.TechnicianName);
        Assert.Equal(1, unassigned.PendingCycles);
        Assert.Null(unassigned.CompliancePercentage);
    }

    private static Task<HelpDesk.Backend.Application.DTOs.Common.PagedResponse<TicketSummaryResponse>>
        GetTicketsAsync(
            ITicketReadRepository repository,
            TicketVisibilityScope visibility) =>
        repository.GetPagedAsync(
            new TicketReadFilter(
                visibility,
                null,
                null,
                null,
                null,
                null,
                null,
                null),
            1,
            20,
            default);
}

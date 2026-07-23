using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Users;
using HelpDesk.Backend.Infrastructure.Persistence;
using HelpDesk.Backend.Infrastructure.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Backend.Infrastructure.Tests;

public sealed class PersistenceRoundTripTests
{
    [Fact]
    public async Task UnitOfWork_PersistsAndRehydratesCompleteAggregates()
    {
        await using var database = await InfrastructureTestDatabase.CreateAsync();
        var hardware = InfrastructureTestData.CreateCategory("Hardware");
        var software = InfrastructureTestData.CreateCategory("Software");
        var technician = InfrastructureTestData.CreateTechnician(
            "Técnico Principal",
            "tecnico@helpdesk.local",
            [hardware.Id, software.Id],
            7);
        var replacementTechnician = InfrastructureTestData.CreateTechnician(
            "Técnico Reemplazo",
            "reemplazo@helpdesk.local",
            [hardware.Id],
            3);
        var supervisor = User.CreateSupervisor(
            "Supervisor Hardware",
            "supervisor@helpdesk.local",
            "hash-supervisor",
            hardware.Id,
            InfrastructureTestData.Now);
        var creator = InfrastructureTestData.CreateUser(
            "Cliente Interno",
            "cliente@helpdesk.local");

        await using (var scope = database.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.SupportCategories.AddAsync(hardware, default);
            await unitOfWork.SupportCategories.AddAsync(software, default);
            await unitOfWork.SaveChangesAsync(default);
            await unitOfWork.Users.AddAsync(technician, default);
            await unitOfWork.Users.AddAsync(replacementTechnician, default);
            await unitOfWork.Users.AddAsync(supervisor, default);
            await unitOfWork.Users.AddAsync(creator, default);
            await unitOfWork.SaveChangesAsync(default);

            var ticket = InfrastructureTestData.CreateTicket(
                1,
                creator,
                hardware,
                TicketPriority.Low);
            ticket.Assign(
                technician.Id,
                supervisor.Id,
                InfrastructureTestData.Now.AddMinutes(5));
            ticket.StartProgress(
                technician.Id,
                InfrastructureTestData.Now.AddMinutes(20));
            ticket.Reassign(
                replacementTechnician.Id,
                supervisor.Id,
                "El técnico inicial termina su turno.",
                InfrastructureTestData.Now.AddMinutes(22));
            ticket.StartProgress(
                replacementTechnician.Id,
                InfrastructureTestData.Now.AddMinutes(23));
            ticket.AddGeneralComment(
                creator.Id,
                "Gracias por atender el caso.",
                InfrastructureTestData.Now.AddMinutes(25));
            await unitOfWork.Tickets.AddAsync(ticket, default);
            await unitOfWork.SaveChangesAsync(default);
        }

        await using var verificationScope = database.CreateScope();
        var verificationUnitOfWork =
            verificationScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var loadedTechnician = await verificationUnitOfWork.Users.GetByIdAsync(
            technician.Id,
            default);
        var loadedSupervisor = await verificationUnitOfWork.Users.GetByIdAsync(
            supervisor.Id,
            default);
        var loadedCategory =
            await verificationUnitOfWork.SupportCategories.GetByIdAsync(
                hardware.Id,
                default);
        var loadedTicket = await verificationUnitOfWork.Tickets.GetByIdAsync(
            (await verificationScope.ServiceProvider
                .GetRequiredService<HelpDeskDbContext>()
                .Tickets
                .AsNoTracking()
                .SingleAsync()).Id,
            default);

        Assert.Equal(
            new[] { hardware.Id, software.Id }.Order(),
            loadedTechnician!.TechnicianProfile!.SupportCategoryIds);
        Assert.Equal(7, loadedTechnician.TechnicianProfile.MaxActiveTickets);
        Assert.Equal(
            hardware.Id,
            loadedSupervisor!.SupervisorProfile!.SupportCategoryId);
        Assert.Equal(4, loadedCategory!.SlaPolicies.Count);
        Assert.Equal(TimeSpan.FromHours(36), loadedCategory.GetSlaDuration(TicketPriority.Low));
        Assert.Equal(TicketStatus.InProgress, loadedTicket!.Status);
        Assert.Equal(replacementTechnician.Id, loadedTicket.CurrentTechnicianUserId);
        Assert.Equal(2, loadedTicket.Assignments.Count);
        Assert.Equal(2, loadedTicket.Comments.Count);
        Assert.Equal(5, loadedTicket.StatusHistory.Count);
        Assert.Single(loadedTicket.SlaCycles);
        Assert.Equal(SlaOutcome.Met, loadedTicket.CurrentSlaCycle.Outcome);
        Assert.Equal(
            technician.Id,
            loadedTicket.CurrentSlaCycle.ResponsibleTechnicianUserId);
        Assert.Equal(
            InfrastructureTestData.Now.AddHours(36),
            loadedTicket.CurrentSlaCycle.DeadlineAtUtc);
        Assert.Empty(loadedTicket.DomainEvents);
    }

    [Fact]
    public async Task SoftDeletedTicket_IsHiddenByDefaultQueryFilter()
    {
        await using var database = await InfrastructureTestDatabase.CreateAsync();
        var category = InfrastructureTestData.CreateCategory("Redes");
        var creator = InfrastructureTestData.CreateUser(
            "Usuario Redes",
            "redes@helpdesk.local");
        var ticket = InfrastructureTestData.CreateTicket(1, creator, category);

        await using var scope = database.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.SupportCategories.AddAsync(category, default);
        await unitOfWork.SaveChangesAsync(default);
        await unitOfWork.Users.AddAsync(creator, default);
        await unitOfWork.SaveChangesAsync(default);
        await unitOfWork.Tickets.AddAsync(ticket, default);
        await unitOfWork.SaveChangesAsync(default);
        ticket.DeleteByCreator(
            creator.Id,
            InfrastructureTestData.Now.AddMinutes(1));
        await unitOfWork.SaveChangesAsync(default);

        var context = scope.ServiceProvider.GetRequiredService<HelpDeskDbContext>();
        Assert.Empty(await context.Tickets.AsNoTracking().ToListAsync());
        Assert.Single(
            await context.Tickets
                .IgnoreQueryFilters()
                .AsNoTracking()
                .ToListAsync());
    }

    [Fact]
    public async Task UniqueEmailConstraint_RejectsDuplicates()
    {
        await using var database = await InfrastructureTestDatabase.CreateAsync();
        await using var scope = database.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.Users.AddAsync(
            InfrastructureTestData.CreateUser(
                "Primer Usuario",
                "duplicado@helpdesk.local"),
            default);
        await unitOfWork.SaveChangesAsync(default);
        await unitOfWork.Users.AddAsync(
            InfrastructureTestData.CreateUser(
                "Segundo Usuario",
                "duplicado@helpdesk.local"),
            default);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => unitOfWork.SaveChangesAsync(default));
    }

    [Fact]
    public async Task RowVersion_RejectsStaleAggregateUpdate()
    {
        await using var database = await InfrastructureTestDatabase.CreateAsync();
        var user = InfrastructureTestData.CreateUser(
            "Usuario Concurrente",
            "concurrencia@helpdesk.local");

        await using (var seedScope = database.CreateScope())
        {
            var seedUnitOfWork =
                seedScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await seedUnitOfWork.Users.AddAsync(user, default);
            await seedUnitOfWork.SaveChangesAsync(default);
        }

        await using var firstScope = database.CreateScope();
        await using var secondScope = database.CreateScope();
        var firstUnitOfWork =
            firstScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var secondUnitOfWork =
            secondScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var first = await firstUnitOfWork.Users.GetByIdAsync(user.Id, default);
        var second = await secondUnitOfWork.Users.GetByIdAsync(user.Id, default);

        first!.UpdateIdentity(
            "Primera actualización",
            first.Email.Value,
            InfrastructureTestData.Now.AddMinutes(1));
        await firstUnitOfWork.SaveChangesAsync(default);
        second!.UpdateIdentity(
            "Actualización obsoleta",
            second.Email.Value,
            InfrastructureTestData.Now.AddMinutes(2));

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => secondUnitOfWork.SaveChangesAsync(default));
    }
}

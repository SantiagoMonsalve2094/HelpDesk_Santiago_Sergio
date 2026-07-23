using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Infrastructure.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Backend.Infrastructure.Tests;

public sealed class BackgroundRepositoryTests
{
    [Fact]
    public async Task PendingSlaQuery_LoadsOnlyCyclesThatNeedEvaluation()
    {
        await using var database = await InfrastructureTestDatabase.CreateAsync();
        var category = InfrastructureTestData.CreateCategory("Monitoreo");
        var creator = InfrastructureTestData.CreateUser(
            "Usuario Monitoreo",
            "monitoreo@helpdesk.local");
        var technician = InfrastructureTestData.CreateTechnician(
            "Técnico Monitoreo",
            "tecnico.monitoreo@helpdesk.local",
            [category.Id]);
        var pending = InfrastructureTestData.CreateTicket(1, creator, category);
        var attended = InfrastructureTestData.CreateTicket(2, creator, category);
        attended.Assign(
            technician.Id,
            technician.Id,
            InfrastructureTestData.Now.AddMinutes(1));
        attended.StartProgress(
            technician.Id,
            InfrastructureTestData.Now.AddMinutes(10));

        await using var scope = database.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.SupportCategories.AddAsync(category, default);
        await unitOfWork.SaveChangesAsync(default);
        await unitOfWork.Users.AddAsync(creator, default);
        await unitOfWork.Users.AddAsync(technician, default);
        await unitOfWork.SaveChangesAsync(default);
        await unitOfWork.Tickets.AddAsync(pending, default);
        await unitOfWork.Tickets.AddAsync(attended, default);
        await unitOfWork.SaveChangesAsync(default);

        var tickets = await unitOfWork.Tickets.GetPendingSlaTicketsAsync(
            InfrastructureTestData.Now.AddHours(2),
            100,
            default);

        var ticket = Assert.Single(tickets);
        Assert.Equal(pending.Id, ticket.Id);
        Assert.True(ticket.EvaluateSla(InfrastructureTestData.Now.AddHours(2)));
        await unitOfWork.SaveChangesAsync(default);
        Assert.Equal(SlaOutcome.Breached, ticket.CurrentSlaCycle.Outcome);
    }

    [Fact]
    public async Task AutomaticClosureQuery_LoadsResolvedTicketAfterFortyEightHours()
    {
        await using var database = await InfrastructureTestDatabase.CreateAsync();
        var category = InfrastructureTestData.CreateCategory("Aplicaciones");
        var creator = InfrastructureTestData.CreateUser(
            "Usuario Aplicaciones",
            "aplicaciones@helpdesk.local");
        var technician = InfrastructureTestData.CreateTechnician(
            "Técnico Aplicaciones",
            "tecnico.aplicaciones@helpdesk.local",
            [category.Id]);
        var ticket = InfrastructureTestData.CreateTicket(1, creator, category);
        ticket.Assign(
            technician.Id,
            technician.Id,
            InfrastructureTestData.Now.AddMinutes(1));
        ticket.StartProgress(
            technician.Id,
            InfrastructureTestData.Now.AddMinutes(5));
        var resolvedAt = InfrastructureTestData.Now.AddMinutes(10);
        ticket.Resolve(
            technician.Id,
            "La aplicación fue restablecida.",
            resolvedAt);

        await using var scope = database.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.SupportCategories.AddAsync(category, default);
        await unitOfWork.SaveChangesAsync(default);
        await unitOfWork.Users.AddAsync(creator, default);
        await unitOfWork.Users.AddAsync(technician, default);
        await unitOfWork.SaveChangesAsync(default);
        await unitOfWork.Tickets.AddAsync(ticket, default);
        await unitOfWork.SaveChangesAsync(default);

        var tickets =
            await unitOfWork.Tickets.GetResolvedForAutomaticClosureAsync(
                resolvedAt.AddHours(48),
                100,
                default);
        var loaded = Assert.Single(tickets);
        loaded.CloseAutomatically(resolvedAt.AddHours(48));
        await unitOfWork.SaveChangesAsync(default);

        Assert.Equal(TicketStatus.Closed, loaded.Status);
        Assert.Equal(resolvedAt.AddHours(48), loaded.ClosedAtUtc);
    }
}

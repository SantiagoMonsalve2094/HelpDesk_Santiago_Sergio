using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Infrastructure.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Backend.Infrastructure.Tests;

public sealed class TicketNumberSequenceTests
{
    [Fact]
    public async Task FailedTicketInsert_RollsBackSequenceValue()
    {
        await using var database = await InfrastructureTestDatabase.CreateAsync();
        var category = InfrastructureTestData.CreateCategory("Impresoras");
        var creator = InfrastructureTestData.CreateUser(
            "Sergio Otalvaro",
            "impresoras@helpdesk.local");
        var existingTicket = InfrastructureTestData.CreateTicket(
            1,
            creator,
            category);

        await using (var seedScope = database.CreateScope())
        {
            var unitOfWork =
                seedScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.SupportCategories.AddAsync(category, default);
            await unitOfWork.SaveChangesAsync(default);
            await unitOfWork.Users.AddAsync(creator, default);
            await unitOfWork.SaveChangesAsync(default);
            await unitOfWork.Tickets.AddAsync(existingTicket, default);
            await unitOfWork.SaveChangesAsync(default);
        }

        await using (var failingScope = database.CreateScope())
        {
            var unitOfWork =
                failingScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var sequence = await unitOfWork.TicketNumbers.GetNextAsync(
                InfrastructureTestData.Now.Year,
                default);
            var duplicate = InfrastructureTestData.CreateTicket(
                sequence,
                creator,
                category);
            await unitOfWork.Tickets.AddAsync(duplicate, default);

            await Assert.ThrowsAsync<DbUpdateException>(
                () => unitOfWork.SaveChangesAsync(default));
        }

        await using var verificationScope = database.CreateScope();
        var verificationUnitOfWork =
            verificationScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var reusedSequence =
            await verificationUnitOfWork.TicketNumbers.GetNextAsync(
                InfrastructureTestData.Now.Year,
                default);
        await verificationUnitOfWork.SaveChangesAsync(default);

        Assert.Equal(1, reusedSequence);
    }

    [Fact]
    public async Task ConcurrentReservations_ReturnUniqueConsecutiveValues()
    {
        await using var database = await InfrastructureTestDatabase.CreateAsync();

        async Task<int> ReserveAsync()
        {
            await using var scope = database.CreateScope();
            var unitOfWork =
                scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var value = await unitOfWork.TicketNumbers.GetNextAsync(
                InfrastructureTestData.Now.Year,
                default);
            await unitOfWork.SaveChangesAsync(default);
            return value;
        }

        var values = await Task.WhenAll(ReserveAsync(), ReserveAsync());

        Assert.Equal(new[] { 1, 2 }, values.Order());
    }

    [Fact]
    public async Task RepositoryOperations_PropagateCancellation()
    {
        await using var database = await InfrastructureTestDatabase.CreateAsync();
        await using var scope = database.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => unitOfWork.Users.GetByIdAsync(
                Guid.NewGuid(),
                cancellation.Token));
    }
}

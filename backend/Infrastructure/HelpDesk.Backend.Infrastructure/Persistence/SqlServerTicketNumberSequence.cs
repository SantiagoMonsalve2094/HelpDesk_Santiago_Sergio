using System.Data;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HelpDesk.Backend.Infrastructure.Persistence;

internal sealed class SqlServerTicketNumberSequence(HelpDeskDbContext dbContext)
    : ITicketNumberSequence, IAsyncDisposable
{
    private IDbContextTransaction? _transaction;

    public async Task<int> GetNextAsync(
        int year,
        CancellationToken cancellationToken)
    {
        if (year is < 2000 or > 9999)
        {
            throw new ArgumentOutOfRangeException(
                nameof(year),
                "El año debe estar entre 2000 y 9999.");
        }

        _transaction ??= await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var state = dbContext.TicketNumberSequences.Local
            .SingleOrDefault(sequence => sequence.Year == year);
        state ??= await dbContext.TicketNumberSequences
            .FromSqlInterpolated(
                $"SELECT * FROM ticket_number_sequences WITH (UPDLOCK, HOLDLOCK) WHERE [year] = {year}")
            .SingleOrDefaultAsync(cancellationToken);

        if (state is null)
        {
            state = TicketNumberSequenceState.Create(year);
            await dbContext.TicketNumberSequences.AddAsync(state, cancellationToken);
        }

        var next = state.GetNext();
        if (next > 999999)
        {
            throw new InvalidOperationException(
                $"Se agotó el consecutivo anual de tickets para {year}.");
        }

        return next;
    }

    internal async Task CommitAsync(CancellationToken cancellationToken)
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.CommitAsync(cancellationToken);
        await DisposeTransactionAsync();
    }

    internal async Task RollbackAsync(CancellationToken cancellationToken)
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.RollbackAsync(cancellationToken);
        await DisposeTransactionAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(CancellationToken.None);
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.DisposeAsync();
        _transaction = null;
    }
}

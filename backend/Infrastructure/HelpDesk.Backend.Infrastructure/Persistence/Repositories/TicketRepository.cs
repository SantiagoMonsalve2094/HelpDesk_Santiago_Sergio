using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Backend.Infrastructure.Persistence.Repositories;

internal sealed class TicketRepository(HelpDeskDbContext dbContext)
    : ITicketRepository
{
    public Task<Ticket?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        FullGraph()
            .SingleOrDefaultAsync(ticket => ticket.Id == id, cancellationToken);

    public Task AddAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return dbContext.Tickets.AddAsync(ticket, cancellationToken).AsTask();
    }

    public Task<int> CountActiveByTechnicianAsync(
        Guid technicianUserId,
        CancellationToken cancellationToken) =>
        dbContext.Tickets.CountAsync(
            ticket =>
                ticket.CurrentTechnicianUserId == technicianUserId &&
                (ticket.Status == TicketStatus.Assigned ||
                 ticket.Status == TicketStatus.InProgress ||
                 ticket.Status == TicketStatus.Reopened),
            cancellationToken);

    public async Task<IReadOnlyList<Ticket>> GetPendingSlaTicketsAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken)
    {
        return await FullGraph()
            .Where(ticket =>
                ticket.SlaCycles
                    .OrderByDescending(cycle => cycle.StartedAtUtc)
                    .Select(cycle => cycle.Outcome)
                    .First() == SlaOutcome.Pending &&
                ticket.SlaCycles
                    .OrderByDescending(cycle => cycle.StartedAtUtc)
                    .Select(cycle => cycle.DeadlineAtUtc)
                    .First() < now)
            .OrderBy(ticket =>
                ticket.SlaCycles
                    .OrderByDescending(cycle => cycle.StartedAtUtc)
                    .Select(cycle => cycle.DeadlineAtUtc)
                    .First())
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetResolvedForAutomaticClosureAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var resolvedBeforeUtc = now.AddHours(-48);
        return await FullGraph()
            .Where(ticket =>
                ticket.Status == TicketStatus.Resolved &&
                ticket.ResolvedAtUtc != null &&
                ticket.ResolvedAtUtc <= resolvedBeforeUtc)
            .OrderBy(ticket => ticket.ResolvedAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Ticket> FullGraph() =>
        dbContext.Tickets
            .Include(ticket => ticket.Assignments)
            .Include(ticket => ticket.Comments)
            .Include(ticket => ticket.StatusHistory)
            .Include(ticket => ticket.SlaCycles)
            .AsSplitQuery();
}

using HelpDesk.Backend.Domain.Tickets;

namespace HelpDesk.Backend.Application.Abstractions.Persistence;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken);
    Task<int> CountActiveByTechnicianAsync(Guid technicianUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Ticket>> GetPendingSlaTicketsAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<Ticket>> GetResolvedForAutomaticClosureAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken);
}

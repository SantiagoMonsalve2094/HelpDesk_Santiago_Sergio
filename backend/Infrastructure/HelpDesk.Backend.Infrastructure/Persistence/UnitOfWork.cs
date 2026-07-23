using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Infrastructure.Persistence.Repositories;

namespace HelpDesk.Backend.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly HelpDeskDbContext _dbContext;
    private readonly SqlServerTicketNumberSequence _ticketNumberSequence;

    public UnitOfWork(
        HelpDeskDbContext dbContext,
        SqlServerTicketNumberSequence ticketNumberSequence)
    {
        _dbContext = dbContext;
        _ticketNumberSequence = ticketNumberSequence;
        Users = new UserRepository(dbContext);
        SupportCategories = new SupportCategoryRepository(dbContext);
        Tickets = new TicketRepository(dbContext);
    }

    public IUserRepository Users { get; }
    public ISupportCategoryRepository SupportCategories { get; }
    public ITicketRepository Tickets { get; }
    public ITicketNumberSequence TicketNumbers => _ticketNumberSequence;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var affectedRows = await _dbContext.SaveChangesAsync(cancellationToken);
            await _ticketNumberSequence.CommitAsync(cancellationToken);
            return affectedRows;
        }
        catch
        {
            await _ticketNumberSequence.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}

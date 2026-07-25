namespace HelpDesk.Backend.Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ISupportCategoryRepository SupportCategories { get; }
    ITicketRepository Tickets { get; }
    ITicketNumberSequence TicketNumbers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

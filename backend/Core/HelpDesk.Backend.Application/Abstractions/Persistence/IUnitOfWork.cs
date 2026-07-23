namespace HelpDesk.Backend.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ISupportCategoryRepository SupportCategories { get; }
    ITicketRepository Tickets { get; }
    ITicketNumberSequence TicketNumbers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

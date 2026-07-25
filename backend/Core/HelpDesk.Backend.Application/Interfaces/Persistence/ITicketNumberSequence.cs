namespace HelpDesk.Backend.Application.Interfaces.Persistence;

public interface ITicketNumberSequence
{
    Task<int> GetNextAsync(int year, CancellationToken cancellationToken);
}

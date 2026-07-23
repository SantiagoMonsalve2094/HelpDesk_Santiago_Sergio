namespace HelpDesk.Backend.Application.Abstractions.Persistence;

public interface ITicketNumberSequence
{
    Task<int> GetNextAsync(int year, CancellationToken cancellationToken);
}

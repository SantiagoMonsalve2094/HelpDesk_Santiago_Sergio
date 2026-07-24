namespace HelpDesk.Backend.Application.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

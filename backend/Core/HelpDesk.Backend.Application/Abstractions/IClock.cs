namespace HelpDesk.Backend.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

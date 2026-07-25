using HelpDesk.Backend.Application.Interfaces;

namespace HelpDesk.Backend.Infrastructure.Services;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

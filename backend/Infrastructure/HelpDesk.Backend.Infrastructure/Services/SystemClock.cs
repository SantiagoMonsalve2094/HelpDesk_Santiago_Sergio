using HelpDesk.Backend.Application.Abstractions;

namespace HelpDesk.Backend.Infrastructure.Services;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

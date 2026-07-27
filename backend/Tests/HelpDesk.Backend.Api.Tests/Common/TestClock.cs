using HelpDesk.Backend.Application.Interfaces;

namespace HelpDesk.Backend.Api.Tests.Common;

internal sealed class TestClock : IClock
{
    internal TestClock(DateTimeOffset initialUtc)
    {
        UtcNow = initialUtc;
    }

    public DateTimeOffset UtcNow { get; private set; }

    internal void Advance(TimeSpan duration)
    {
        UtcNow = UtcNow.Add(duration);
    }
}

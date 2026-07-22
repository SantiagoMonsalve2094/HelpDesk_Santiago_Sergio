using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.ValueObjects;

namespace HelpDesk.Backend.Domain.Tests.ValueObjects;

public sealed class TicketNumberTests
{
    [Fact]
    public void Create_UsesExpectedBusinessFormat()
    {
        var number = TicketNumber.Create(2026, 42);

        Assert.Equal("HD-2026-000042", number.Value);
        Assert.Equal(2026, number.Year);
        Assert.Equal(42, number.Sequence);
    }

    [Theory]
    [InlineData("2026-000001")]
    [InlineData("HD-26-1")]
    [InlineData("")]
    public void Parse_RejectsInvalidFormat(string value)
    {
        Assert.Throws<DomainException>(() => TicketNumber.Parse(value));
    }
}

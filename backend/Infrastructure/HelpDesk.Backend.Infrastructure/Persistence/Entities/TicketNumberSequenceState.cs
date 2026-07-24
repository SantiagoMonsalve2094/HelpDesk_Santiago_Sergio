namespace HelpDesk.Backend.Infrastructure.Persistence.Entities;

internal sealed class TicketNumberSequenceState
{
    private TicketNumberSequenceState()
    {
    }

    private TicketNumberSequenceState(int year)
    {
        Year = year;
    }

    public int Year { get; private set; }
    public int LastValue { get; private set; }

    public static TicketNumberSequenceState Create(int year) => new(year);

    public int GetNext()
    {
        LastValue++;
        return LastValue;
    }
}

using System.Globalization;
using System.Text.RegularExpressions;
using HelpDesk.Backend.Domain.Common;

namespace HelpDesk.Backend.Domain.ValueObjects;

public sealed partial record TicketNumber
{
    private TicketNumber(string value, int year, int sequence)
    {
        Value = value;
        Year = year;
        Sequence = sequence;
    }

    public string Value { get; }
    public int Year { get; }
    public int Sequence { get; }

    public static TicketNumber Create(int year, int sequence)
    {
        if (year < 2000 || year > 9999)
        {
            throw new DomainException("INVALID_TICKET_YEAR", "El año del ticket debe estar entre 2000 y 9999.");
        }

        if (sequence < 1 || sequence > 999999)
        {
            throw new DomainException("INVALID_TICKET_SEQUENCE", "El consecutivo debe estar entre 1 y 999999.");
        }

        return new TicketNumber(
            $"HD-{year.ToString("0000", CultureInfo.InvariantCulture)}-{sequence.ToString("000000", CultureInfo.InvariantCulture)}",
            year,
            sequence);
    }

    public static TicketNumber Parse(string? value)
    {
        var normalized = Guard.Required(
            value,
            14,
            "INVALID_TICKET_NUMBER",
            "El número de ticket es obligatorio y debe usar el formato HD-AAAA-NNNNNN.");

        var match = TicketNumberPattern().Match(normalized);
        if (!match.Success)
        {
            throw new DomainException("INVALID_TICKET_NUMBER", "El número de ticket debe usar el formato HD-AAAA-NNNNNN.");
        }

        var year = int.Parse(match.Groups["year"].Value, CultureInfo.InvariantCulture);
        var sequence = int.Parse(match.Groups["sequence"].Value, CultureInfo.InvariantCulture);
        return Create(year, sequence);
    }

    public override string ToString() => Value;

    [GeneratedRegex("^HD-(?<year>\\d{4})-(?<sequence>\\d{6})$", RegexOptions.CultureInvariant)]
    private static partial Regex TicketNumberPattern();
}

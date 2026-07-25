namespace HelpDesk.Backend.Domain.Common;

internal static class Guard
{
    public static Guid Required(Guid value, string code, string message)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException(code, message);
        }

        return value;
    }

    public static string Required(string? value, int maxLength, string code, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(code, message);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException(code, message);
        }

        return normalized;
    }

    public static TimeSpan PositiveDuration(TimeSpan value, string code, string message)
    {
        if (value <= TimeSpan.Zero)
        {
            throw new DomainException(code, message);
        }

        return value;
    }
}

using System.Net.Mail;
using HelpDesk.Backend.Domain.Common;

namespace HelpDesk.Backend.Domain.ValueObjects;

public sealed record EmailAddress
{
    private const int MaxLength = 254;

    private EmailAddress(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static EmailAddress Create(string? value)
    {
        var normalized = Guard.Required(
            value,
            MaxLength,
            "INVALID_EMAIL",
            "El correo electrónico es obligatorio y debe tener máximo 254 caracteres.")
            .ToLowerInvariant();

        if (!MailAddress.TryCreate(normalized, out var address) ||
            !string.Equals(address.Address, normalized, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("INVALID_EMAIL", "El correo electrónico no tiene un formato válido.");
        }

        return new EmailAddress(normalized);
    }

    public override string ToString() => Value;
}

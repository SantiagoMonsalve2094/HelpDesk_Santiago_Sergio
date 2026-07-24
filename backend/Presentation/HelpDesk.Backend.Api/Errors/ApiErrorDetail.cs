namespace HelpDesk.Backend.Api.Errors;

public sealed record ApiErrorDetail(
    string Code,
    string Message,
    string? Field);

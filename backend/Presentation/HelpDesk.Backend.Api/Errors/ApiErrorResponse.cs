namespace HelpDesk.Backend.Api.Errors;

public sealed record ApiErrorDetail(
    string Code,
    string Message,
    string? Field);

public sealed record ApiErrorResponse(
    int Status,
    string Title,
    string Data,
    string TraceId,
    IReadOnlyCollection<ApiErrorDetail> Errors);

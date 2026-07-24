namespace HelpDesk.Backend.Api.Errors;

public sealed record ApiErrorResponse(
    int Status,
    string Title,
    string Data,
    string TraceId,
    IReadOnlyCollection<ApiErrorDetail> Errors);

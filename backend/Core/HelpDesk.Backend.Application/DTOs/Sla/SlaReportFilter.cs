namespace HelpDesk.Backend.Application.DTOs.Sla;

public sealed record SlaReportFilter(
    Guid? SupportCategoryId,
    Guid? TechnicianUserId,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc);

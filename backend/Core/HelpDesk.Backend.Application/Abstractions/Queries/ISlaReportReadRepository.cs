using HelpDesk.Backend.Application.Features.Tickets.Models;

namespace HelpDesk.Backend.Application.Abstractions.Queries;

public sealed record SlaReportFilter(
    Guid? SupportCategoryId,
    Guid? TechnicianUserId,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc);

public interface ISlaReportReadRepository
{
    Task<SlaReportResponse> GetReportAsync(
        SlaReportFilter filter,
        CancellationToken cancellationToken);
}

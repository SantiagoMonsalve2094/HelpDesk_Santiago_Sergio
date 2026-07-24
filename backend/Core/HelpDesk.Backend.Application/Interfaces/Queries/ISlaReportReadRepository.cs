using HelpDesk.Backend.Application.DTOs.Sla;

namespace HelpDesk.Backend.Application.Interfaces.Queries;

public interface ISlaReportReadRepository
{
    Task<SlaReportResponse> GetReportAsync(
        SlaReportFilter filter,
        CancellationToken cancellationToken);
}

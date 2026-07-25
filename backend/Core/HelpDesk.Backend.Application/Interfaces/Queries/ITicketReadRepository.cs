using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;

namespace HelpDesk.Backend.Application.Interfaces.Queries;

public interface ITicketReadRepository
{
    Task<PagedResponse<TicketSummaryResponse>> GetPagedAsync(
        TicketReadFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AssignableTechnicianResponse>> GetAssignableTechniciansAsync(
        Guid supportCategoryId,
        CancellationToken cancellationToken);

    Task<PagedResponse<SlaAlertResponse>> GetSlaAlertsAsync(
        TicketVisibilityScope visibility,
        Guid? supportCategoryId,
        DateTimeOffset asOfUtc,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}

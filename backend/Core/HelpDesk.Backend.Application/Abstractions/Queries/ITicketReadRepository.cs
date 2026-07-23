using HelpDesk.Backend.Application.Common.Models;
using HelpDesk.Backend.Application.Features.Tickets.Models;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Application.Abstractions.Queries;

public sealed record TicketVisibilityScope(
    Guid ActorUserId,
    UserRole ActorRole,
    Guid? SupervisorSupportCategoryId);

public sealed record TicketReadFilter(
    TicketVisibilityScope Visibility,
    TicketStatus? Status,
    TicketPriority? Priority,
    Guid? SupportCategoryId,
    Guid? TechnicianUserId,
    bool? IsOverdue,
    DateTimeOffset? CreatedFromUtc,
    DateTimeOffset? CreatedToUtc);

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

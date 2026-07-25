using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.Common.Validation;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Sla.Queries.GetSlaAlerts;

public sealed class GetSlaAlertsHandler(
    IUnitOfWork unitOfWork,
    ITicketReadRepository readRepository,
    IClock clock,
    IValidator<GetSlaAlertsQuery> validator)
    : IRequestHandler<GetSlaAlertsQuery, PagedResponse<SlaAlertResponse>>
{
    public async Task<PagedResponse<SlaAlertResponse>> Handle(
        GetSlaAlertsQuery request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.ActorUserId,
            cancellationToken);
        var visibility = ApplicationAccess.CreateTicketVisibilityScope(actor);

        return await readRepository.GetSlaAlertsAsync(
            visibility,
            request.SupportCategoryId,
            clock.UtcNow,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}

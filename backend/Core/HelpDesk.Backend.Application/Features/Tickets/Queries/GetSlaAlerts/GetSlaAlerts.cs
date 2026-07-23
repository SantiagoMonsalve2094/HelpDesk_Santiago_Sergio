using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Abstractions.Queries;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Application.Common.Models;
using HelpDesk.Backend.Application.Common.Validation;
using HelpDesk.Backend.Application.Features.Tickets.Models;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Queries.GetSlaAlerts;

public sealed record GetSlaAlertsQuery(
    Guid ActorUserId,
    Guid? SupportCategoryId = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResponse<SlaAlertResponse>>;

public sealed class GetSlaAlertsValidator : AbstractValidator<GetSlaAlertsQuery>
{
    public GetSlaAlertsValidator()
    {
        RuleFor(query => query.ActorUserId).NotEmpty();
        this.ApplyPaginationRules(query => query.PageNumber, query => query.PageSize);
    }
}

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

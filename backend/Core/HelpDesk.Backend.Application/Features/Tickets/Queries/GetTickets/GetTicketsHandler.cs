using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.Common.Validation;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Queries.GetTickets;

public sealed class GetTicketsHandler(
    IUnitOfWork unitOfWork,
    ITicketReadRepository readRepository,
    IValidator<GetTicketsQuery> validator)
    : IRequestHandler<GetTicketsQuery, PagedResponse<TicketSummaryResponse>>
{
    public async Task<PagedResponse<TicketSummaryResponse>> Handle(
        GetTicketsQuery request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.ActorUserId,
            cancellationToken);
        var visibility = ApplicationAccess.CreateTicketVisibilityScope(actor);
        var filter = new TicketReadFilter(
            visibility,
            request.Status,
            request.Priority,
            request.SupportCategoryId,
            request.TechnicianUserId,
            request.IsOverdue,
            request.CreatedFromUtc,
            request.CreatedToUtc);

        return await readRepository.GetPagedAsync(
            filter,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}

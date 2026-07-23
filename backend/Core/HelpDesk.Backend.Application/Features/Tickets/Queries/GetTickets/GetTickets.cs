using FluentValidation;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Abstractions.Queries;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Application.Common.Models;
using HelpDesk.Backend.Application.Common.Validation;
using HelpDesk.Backend.Application.Features.Tickets.Models;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Queries.GetTickets;

public sealed record GetTicketsQuery(
    Guid ActorUserId,
    TicketStatus? Status = null,
    TicketPriority? Priority = null,
    Guid? SupportCategoryId = null,
    Guid? TechnicianUserId = null,
    bool? IsOverdue = null,
    DateTimeOffset? CreatedFromUtc = null,
    DateTimeOffset? CreatedToUtc = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResponse<TicketSummaryResponse>>;

public sealed class GetTicketsValidator : AbstractValidator<GetTicketsQuery>
{
    public GetTicketsValidator()
    {
        RuleFor(query => query.ActorUserId).NotEmpty();
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
        RuleFor(query => query.Priority).IsInEnum().When(query => query.Priority.HasValue);
        RuleFor(query => query)
            .Must(query =>
                query.CreatedFromUtc is null ||
                query.CreatedToUtc is null ||
                query.CreatedFromUtc <= query.CreatedToUtc)
            .WithMessage("La fecha inicial no puede ser posterior a la fecha final.");
        this.ApplyPaginationRules(query => query.PageNumber, query => query.PageSize);
    }
}

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

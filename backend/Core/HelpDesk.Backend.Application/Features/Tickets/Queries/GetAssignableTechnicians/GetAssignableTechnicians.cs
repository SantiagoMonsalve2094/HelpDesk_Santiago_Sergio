using FluentValidation;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Abstractions.Queries;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Application.Features.Tickets.Models;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Queries.GetAssignableTechnicians;

public sealed record GetAssignableTechniciansQuery(
    Guid ActorUserId,
    Guid TicketId) : IRequest<IReadOnlyList<AssignableTechnicianResponse>>;

public sealed class GetAssignableTechniciansValidator
    : AbstractValidator<GetAssignableTechniciansQuery>
{
    public GetAssignableTechniciansValidator()
    {
        RuleFor(query => query.ActorUserId).NotEmpty();
        RuleFor(query => query.TicketId).NotEmpty();
    }
}

public sealed class GetAssignableTechniciansHandler(
    IUnitOfWork unitOfWork,
    ITicketReadRepository readRepository,
    IValidator<GetAssignableTechniciansQuery> validator)
    : IRequestHandler<GetAssignableTechniciansQuery, IReadOnlyList<AssignableTechnicianResponse>>
{
    public async Task<IReadOnlyList<AssignableTechnicianResponse>> Handle(
        GetAssignableTechniciansQuery request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.ActorUserId,
            cancellationToken);
        var ticket = await ApplicationAccess.GetTicketAsync(
            unitOfWork,
            request.TicketId,
            cancellationToken);
        TicketApplicationAccess.EnsureCanManageAssignment(actor, ticket);

        return await readRepository.GetAssignableTechniciansAsync(
            ticket.SupportCategoryId,
            cancellationToken);
    }
}

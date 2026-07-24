using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Queries.GetAssignableTechnicians;

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

using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Queries.GetTicketById;

public sealed class GetTicketByIdHandler(
    IUnitOfWork unitOfWork,
    IValidator<GetTicketByIdQuery> validator)
    : IRequestHandler<GetTicketByIdQuery, TicketDetailsResponse>
{
    public async Task<TicketDetailsResponse> Handle(
        GetTicketByIdQuery request,
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
        TicketApplicationAccess.EnsureCanView(actor, ticket);
        return TicketMapper.ToDetails(ticket);
    }
}

using FluentValidation;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Application.Features.Tickets.Models;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Queries.GetTicketById;

public sealed record GetTicketByIdQuery(
    Guid ActorUserId,
    Guid TicketId) : IRequest<TicketDetailsResponse>;

public sealed class GetTicketByIdValidator : AbstractValidator<GetTicketByIdQuery>
{
    public GetTicketByIdValidator()
    {
        RuleFor(query => query.ActorUserId).NotEmpty();
        RuleFor(query => query.TicketId).NotEmpty();
    }
}

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

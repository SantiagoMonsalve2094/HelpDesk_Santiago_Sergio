using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.ResolveTicket;

public sealed class ResolveTicketHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<ResolveTicketCommand> validator)
    : IRequestHandler<ResolveTicketCommand>
{
    public async Task Handle(ResolveTicketCommand request, CancellationToken cancellationToken)
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
        TicketApplicationAccess.EnsureCanStartOrResolve(actor, ticket);

        ticket.Resolve(actor.Id, request.ResolutionComment, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.UpdateTicket;

public sealed class UpdateTicketHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<UpdateTicketCommand> validator)
    : IRequestHandler<UpdateTicketCommand>
{
    public async Task Handle(
        UpdateTicketCommand request,
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

        ticket.UpdateDescription(request.Subject, request.Description, actor.Id, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

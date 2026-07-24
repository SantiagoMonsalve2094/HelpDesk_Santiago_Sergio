using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.AddTicketComment;

public sealed class AddTicketCommentHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<AddTicketCommentCommand> validator)
    : IRequestHandler<AddTicketCommentCommand>
{
    public async Task Handle(
        AddTicketCommentCommand request,
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
        TicketApplicationAccess.EnsureCanComment(actor, ticket);

        ticket.AddGeneralComment(actor.Id, request.Body, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.DeleteTicket;

public sealed record DeleteTicketCommand(Guid ActorUserId, Guid TicketId) : IRequest;

public sealed class DeleteTicketValidator : AbstractValidator<DeleteTicketCommand>
{
    public DeleteTicketValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.TicketId).NotEmpty();
    }
}

public sealed class DeleteTicketHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<DeleteTicketCommand> validator)
    : IRequestHandler<DeleteTicketCommand>
{
    public async Task Handle(DeleteTicketCommand request, CancellationToken cancellationToken)
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

        ticket.DeleteByCreator(actor.Id, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

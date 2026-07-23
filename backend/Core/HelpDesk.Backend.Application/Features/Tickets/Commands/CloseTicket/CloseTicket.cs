using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.CloseTicket;

public sealed record CloseTicketCommand(Guid ActorUserId, Guid TicketId) : IRequest;

public sealed class CloseTicketValidator : AbstractValidator<CloseTicketCommand>
{
    public CloseTicketValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.TicketId).NotEmpty();
    }
}

public sealed class CloseTicketHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<CloseTicketCommand> validator)
    : IRequestHandler<CloseTicketCommand>
{
    public async Task Handle(CloseTicketCommand request, CancellationToken cancellationToken)
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

        ticket.CloseByCreator(actor.Id, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

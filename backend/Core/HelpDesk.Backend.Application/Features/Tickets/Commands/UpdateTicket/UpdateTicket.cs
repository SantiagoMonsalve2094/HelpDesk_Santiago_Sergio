using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.UpdateTicket;

public sealed record UpdateTicketCommand(
    Guid ActorUserId,
    Guid TicketId,
    string Subject,
    string Description) : IRequest;

public sealed class UpdateTicketValidator : AbstractValidator<UpdateTicketCommand>
{
    public UpdateTicketValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.TicketId).NotEmpty();
        RuleFor(command => command.Subject).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).NotEmpty().MaximumLength(4000);
    }
}

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

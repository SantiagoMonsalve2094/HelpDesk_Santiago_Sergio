using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.ResolveTicket;

public sealed record ResolveTicketCommand(
    Guid ActorUserId,
    Guid TicketId,
    string ResolutionComment) : IRequest;

public sealed class ResolveTicketValidator : AbstractValidator<ResolveTicketCommand>
{
    public ResolveTicketValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.TicketId).NotEmpty();
        RuleFor(command => command.ResolutionComment).NotEmpty().MaximumLength(4000);
    }
}

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

using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.AddTicketComment;

public sealed record AddTicketCommentCommand(
    Guid ActorUserId,
    Guid TicketId,
    string Body) : IRequest;

public sealed class AddTicketCommentValidator : AbstractValidator<AddTicketCommentCommand>
{
    public AddTicketCommentValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.TicketId).NotEmpty();
        RuleFor(command => command.Body).NotEmpty().MaximumLength(4000);
    }
}

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

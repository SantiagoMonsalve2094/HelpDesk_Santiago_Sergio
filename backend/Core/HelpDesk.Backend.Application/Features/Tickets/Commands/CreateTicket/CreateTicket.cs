using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Application.Features.Tickets.Models;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Policies;
using HelpDesk.Backend.Domain.Tickets;
using HelpDesk.Backend.Domain.ValueObjects;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.CreateTicket;

public sealed record CreateTicketCommand(
    Guid ActorUserId,
    string Subject,
    string Description,
    Guid SupportCategoryId,
    TicketPriority Priority) : IRequest<CreatedTicketResponse>;

public sealed class CreateTicketValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.Subject).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.SupportCategoryId).NotEmpty();
        RuleFor(command => command.Priority).IsInEnum();
    }
}

public sealed class CreateTicketHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<CreateTicketCommand> validator)
    : IRequestHandler<CreateTicketCommand, CreatedTicketResponse>
{
    public async Task<CreatedTicketResponse> Handle(
        CreateTicketCommand request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.ActorUserId,
            cancellationToken);
        if (!TicketAccessPolicy.CanCreateTicket(actor))
        {
            throw new UnauthorizedAccessException("Solo un usuario activo puede crear tickets.");
        }

        var category = await ApplicationAccess.GetSupportCategoryAsync(
            unitOfWork,
            request.SupportCategoryId,
            cancellationToken);
        var slaDuration = category.GetSlaDuration(request.Priority);
        var now = clock.UtcNow;
        var sequence = await unitOfWork.TicketNumbers.GetNextAsync(now.Year, cancellationToken);
        var number = TicketNumber.Create(now.Year, sequence);
        var ticket = Ticket.Create(
            number,
            request.Subject,
            request.Description,
            actor.Id,
            category.Id,
            request.Priority,
            slaDuration,
            now);

        await unitOfWork.Tickets.AddAsync(ticket, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreatedTicketResponse(ticket.Id, ticket.Number.Value);
    }
}

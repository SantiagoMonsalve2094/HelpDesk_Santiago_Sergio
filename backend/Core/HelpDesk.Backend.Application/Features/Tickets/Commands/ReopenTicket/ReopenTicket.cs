using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.ReopenTicket;

public sealed record ReopenTicketCommand(Guid ActorUserId, Guid TicketId) : IRequest;

public sealed class ReopenTicketValidator : AbstractValidator<ReopenTicketCommand>
{
    public ReopenTicketValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.TicketId).NotEmpty();
    }
}

public sealed class ReopenTicketHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<ReopenTicketCommand> validator)
    : IRequestHandler<ReopenTicketCommand>
{
    public async Task Handle(ReopenTicketCommand request, CancellationToken cancellationToken)
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

        var category = await ApplicationAccess.GetSupportCategoryAsync(
            unitOfWork,
            ticket.SupportCategoryId,
            cancellationToken);
        var previousTechnicianHasCapacity = false;
        if (ticket.CurrentTechnicianUserId is Guid technicianUserId)
        {
            var technician = await ApplicationAccess.GetUserAsync(
                unitOfWork,
                technicianUserId,
                cancellationToken);
            var activeTickets = await unitOfWork.Tickets.CountActiveByTechnicianAsync(
                technicianUserId,
                cancellationToken);
            previousTechnicianHasCapacity =
                technician.IsActive &&
                technician.SupportsCategory(ticket.SupportCategoryId) &&
                technician.TechnicianProfile is not null &&
                activeTickets < technician.TechnicianProfile.MaxActiveTickets;
        }

        ticket.ReopenByCreator(
            actor.Id,
            previousTechnicianHasCapacity,
            category.GetSlaDuration(ticket.Priority),
            clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

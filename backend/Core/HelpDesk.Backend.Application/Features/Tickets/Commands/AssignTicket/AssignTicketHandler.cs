using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.AssignTicket;

public sealed class AssignTicketHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<AssignTicketCommand> validator)
    : IRequestHandler<AssignTicketCommand>
{
    public async Task Handle(AssignTicketCommand request, CancellationToken cancellationToken)
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
        TicketApplicationAccess.EnsureCanManageAssignment(actor, ticket);
        var technician = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.TechnicianUserId,
            cancellationToken);
        var activeTickets = await unitOfWork.Tickets.CountActiveByTechnicianAsync(
            technician.Id,
            cancellationToken);

        TicketAssignmentPolicy.EnsureCanAssign(actor, ticket, technician, activeTickets);
        ticket.Assign(technician.Id, actor.Id, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

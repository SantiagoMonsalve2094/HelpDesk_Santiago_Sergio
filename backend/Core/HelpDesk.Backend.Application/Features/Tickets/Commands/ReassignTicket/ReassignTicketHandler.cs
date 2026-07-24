using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.ReassignTicket;

public sealed class ReassignTicketHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<ReassignTicketCommand> validator)
    : IRequestHandler<ReassignTicketCommand>
{
    public async Task Handle(
        ReassignTicketCommand request,
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
        TicketApplicationAccess.EnsureCanManageAssignment(actor, ticket);
        var technician = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.NewTechnicianUserId,
            cancellationToken);
        var activeTickets = await unitOfWork.Tickets.CountActiveByTechnicianAsync(
            technician.Id,
            cancellationToken);

        TicketAssignmentPolicy.EnsureCanAssign(actor, ticket, technician, activeTickets);
        ticket.Reassign(technician.Id, actor.Id, request.Reason, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

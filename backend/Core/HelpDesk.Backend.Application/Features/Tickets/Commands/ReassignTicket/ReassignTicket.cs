using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.ReassignTicket;

public sealed record ReassignTicketCommand(
    Guid ActorUserId,
    Guid TicketId,
    Guid NewTechnicianUserId,
    string Reason) : IRequest;

public sealed class ReassignTicketValidator : AbstractValidator<ReassignTicketCommand>
{
    public ReassignTicketValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.TicketId).NotEmpty();
        RuleFor(command => command.NewTechnicianUserId).NotEmpty();
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(1000);
    }
}

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

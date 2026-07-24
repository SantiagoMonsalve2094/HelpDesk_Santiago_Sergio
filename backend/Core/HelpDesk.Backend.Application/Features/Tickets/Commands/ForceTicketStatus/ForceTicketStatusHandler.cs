using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.ForceTicketStatus;

public sealed class ForceTicketStatusHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<ForceTicketStatusCommand> validator)
    : IRequestHandler<ForceTicketStatusCommand>
{
    public async Task Handle(
        ForceTicketStatusCommand request,
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
        TicketApplicationAccess.EnsureCanForceTransition(actor, ticket);

        TimeSpan? newSlaDuration = null;
        if (request.TargetStatus == TicketStatus.Reopened ||
            request.TargetStatus == TicketStatus.Open && !ticket.CurrentSlaCycle.IsPending)
        {
            var category = await ApplicationAccess.GetSupportCategoryAsync(
                unitOfWork,
                ticket.SupportCategoryId,
                cancellationToken);
            newSlaDuration = category.GetSlaDuration(ticket.Priority);
        }

        ticket.ForceTransition(
            request.TargetStatus,
            actor.Id,
            request.Justification,
            clock.UtcNow,
            newSlaDuration);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

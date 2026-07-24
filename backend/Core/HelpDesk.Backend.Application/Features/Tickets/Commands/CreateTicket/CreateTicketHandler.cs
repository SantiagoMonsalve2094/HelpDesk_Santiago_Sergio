using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.Resources;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Policies;
using HelpDesk.Backend.Domain.Aggregates.Tickets;
using HelpDesk.Backend.Domain.ValueObjects;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.CreateTicket;

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
            throw new UnauthorizedAccessException(
                ApplicationMessages.OnlyActiveUserCanCreateTickets);
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

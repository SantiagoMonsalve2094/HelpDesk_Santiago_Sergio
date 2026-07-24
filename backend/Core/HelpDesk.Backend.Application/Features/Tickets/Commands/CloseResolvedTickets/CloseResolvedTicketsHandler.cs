using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.CloseResolvedTickets;

public sealed class CloseResolvedTicketsHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<CloseResolvedTicketsCommand> validator)
    : IRequestHandler<CloseResolvedTicketsCommand, int>
{
    public async Task<int> Handle(
        CloseResolvedTicketsCommand request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var now = clock.UtcNow;
        var tickets = await unitOfWork.Tickets.GetResolvedForAutomaticClosureAsync(
            now,
            request.BatchSize,
            cancellationToken);
        foreach (var ticket in tickets)
        {
            ticket.CloseAutomatically(now);
        }

        if (tickets.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return tickets.Count;
    }
}

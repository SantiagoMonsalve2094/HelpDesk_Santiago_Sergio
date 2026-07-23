using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.CloseResolvedTickets;

public sealed record CloseResolvedTicketsCommand(int BatchSize = 100) : IRequest<int>;

public sealed class CloseResolvedTicketsValidator : AbstractValidator<CloseResolvedTicketsCommand>
{
    public CloseResolvedTicketsValidator()
    {
        RuleFor(command => command.BatchSize).InclusiveBetween(1, 500);
    }
}

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

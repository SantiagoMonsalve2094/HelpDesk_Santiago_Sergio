using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.EvaluatePendingSla;

public sealed record EvaluatePendingSlaCommand(int BatchSize = 100) : IRequest<int>;

public sealed class EvaluatePendingSlaValidator : AbstractValidator<EvaluatePendingSlaCommand>
{
    public EvaluatePendingSlaValidator()
    {
        RuleFor(command => command.BatchSize).InclusiveBetween(1, 500);
    }
}

public sealed class EvaluatePendingSlaHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<EvaluatePendingSlaCommand> validator)
    : IRequestHandler<EvaluatePendingSlaCommand, int>
{
    public async Task<int> Handle(
        EvaluatePendingSlaCommand request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var now = clock.UtcNow;
        var tickets = await unitOfWork.Tickets.GetPendingSlaTicketsAsync(
            now,
            request.BatchSize,
            cancellationToken);
        var breachedCount = tickets.Count(ticket => ticket.EvaluateSla(now));
        if (breachedCount > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return breachedCount;
    }
}

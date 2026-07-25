using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Sla.Commands.EvaluatePendingSla;

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

using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Sla.Commands.EvaluatePendingSla;

public sealed class EvaluatePendingSlaValidator : AbstractValidator<EvaluatePendingSlaCommand>
{
    public EvaluatePendingSlaValidator()
    {
        RuleFor(command => command.BatchSize).InclusiveBetween(1, 500);
    }
}

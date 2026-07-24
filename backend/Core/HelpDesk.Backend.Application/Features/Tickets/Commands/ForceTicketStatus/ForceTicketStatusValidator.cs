using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.ForceTicketStatus;

public sealed class ForceTicketStatusValidator : AbstractValidator<ForceTicketStatusCommand>
{
    public ForceTicketStatusValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.TicketId).NotEmpty();
        RuleFor(command => command.TargetStatus).IsInEnum();
        RuleFor(command => command.Justification).NotEmpty().MaximumLength(1000);
    }
}

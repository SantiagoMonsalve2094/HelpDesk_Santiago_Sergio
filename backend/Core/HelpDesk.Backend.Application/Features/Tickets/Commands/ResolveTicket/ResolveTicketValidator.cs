using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.ResolveTicket;

public sealed class ResolveTicketValidator : AbstractValidator<ResolveTicketCommand>
{
    public ResolveTicketValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.TicketId).NotEmpty();
        RuleFor(command => command.ResolutionComment).NotEmpty().MaximumLength(4000);
    }
}

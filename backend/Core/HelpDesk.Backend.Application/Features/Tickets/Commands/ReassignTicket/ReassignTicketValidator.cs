using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.ReassignTicket;

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

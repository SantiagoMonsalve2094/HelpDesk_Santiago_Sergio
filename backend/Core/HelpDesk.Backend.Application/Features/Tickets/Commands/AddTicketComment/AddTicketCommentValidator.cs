using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.AddTicketComment;

public sealed class AddTicketCommentValidator : AbstractValidator<AddTicketCommentCommand>
{
    public AddTicketCommentValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.TicketId).NotEmpty();
        RuleFor(command => command.Body).NotEmpty().MaximumLength(4000);
    }
}

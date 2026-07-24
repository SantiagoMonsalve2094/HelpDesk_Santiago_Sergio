using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.SetUserActive;

public sealed class SetUserActiveValidator : AbstractValidator<SetUserActiveCommand>
{
    public SetUserActiveValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
    }
}

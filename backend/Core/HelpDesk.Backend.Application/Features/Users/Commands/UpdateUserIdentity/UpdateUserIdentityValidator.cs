using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.UpdateUserIdentity;

public sealed class UpdateUserIdentityValidator : AbstractValidator<UpdateUserIdentityCommand>
{
    public UpdateUserIdentityValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Email).NotEmpty().MaximumLength(254).EmailAddress();
    }
}

using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.ChangeSupervisorCategory;

public sealed class ChangeSupervisorCategoryValidator : AbstractValidator<ChangeSupervisorCategoryCommand>
{
    public ChangeSupervisorCategoryValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.SupervisorUserId).NotEmpty();
        RuleFor(command => command.SupportCategoryId).NotEmpty();
    }
}

using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Commands.UpdateSupportCategory;

public sealed class UpdateSupportCategoryValidator : AbstractValidator<UpdateSupportCategoryCommand>
{
    public UpdateSupportCategoryValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.SupportCategoryId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Description).NotEmpty().MaximumLength(1000);
    }
}

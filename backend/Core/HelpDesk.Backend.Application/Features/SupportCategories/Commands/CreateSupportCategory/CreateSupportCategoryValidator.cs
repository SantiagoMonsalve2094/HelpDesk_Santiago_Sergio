using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Commands.CreateSupportCategory;

public sealed class CreateSupportCategoryValidator : AbstractValidator<CreateSupportCategoryCommand>
{
    public CreateSupportCategoryValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Description).NotEmpty().MaximumLength(1000);
        RuleFor(command => command.LowSla).GreaterThan(TimeSpan.Zero);
        RuleFor(command => command.MediumSla).GreaterThan(TimeSpan.Zero);
        RuleFor(command => command.HighSla).GreaterThan(TimeSpan.Zero);
        RuleFor(command => command.CriticalSla).GreaterThan(TimeSpan.Zero);
    }
}

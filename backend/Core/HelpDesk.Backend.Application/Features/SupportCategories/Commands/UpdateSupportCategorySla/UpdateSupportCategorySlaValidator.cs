using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Commands.UpdateSupportCategorySla;

public sealed class UpdateSupportCategorySlaValidator : AbstractValidator<UpdateSupportCategorySlaCommand>
{
    public UpdateSupportCategorySlaValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.SupportCategoryId).NotEmpty();
        RuleFor(command => command.Priority).IsInEnum();
        RuleFor(command => command.ResponseTime).GreaterThan(TimeSpan.Zero);
    }
}

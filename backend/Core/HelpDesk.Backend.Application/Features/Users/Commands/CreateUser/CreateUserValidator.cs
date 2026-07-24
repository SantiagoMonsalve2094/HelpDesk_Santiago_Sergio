using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.Resources;
using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Policies;
using HelpDesk.Backend.Domain.Aggregates.Users;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(command => command.ActorUserId)
            .NotEmpty()
            .WithErrorCode(ApplicationErrorCodes.Required);
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Email).NotEmpty().MaximumLength(254).EmailAddress();
        RuleFor(command => command.Password).NotEmpty();
        RuleFor(command => command.Role).IsInEnum();
        RuleFor(command => command.SupportCategoryIds).NotNull();
        RuleForEach(command => command.SupportCategoryIds).NotEmpty();

        RuleFor(command => command.SupportCategoryIds)
            .Must((command, categories) => HasValidProfile(command.Role, categories, command.MaxActiveTickets))
            .WithErrorCode(ApplicationErrorCodes.InvalidUserProfile)
            .WithMessage(ApplicationMessages.InvalidUserProfile);
    }

    private static bool HasValidProfile(
        UserRole role,
        IReadOnlyCollection<Guid>? categoryIds,
        int? maxActiveTickets) =>
        categoryIds is not null &&
        role switch
        {
            UserRole.Technician => categoryIds.Count > 0 && maxActiveTickets > 0,
            UserRole.Supervisor => categoryIds.Count == 1 && maxActiveTickets is null,
            UserRole.User or UserRole.SuperAdmin => categoryIds.Count == 0 && maxActiveTickets is null,
            _ => false
        };
}

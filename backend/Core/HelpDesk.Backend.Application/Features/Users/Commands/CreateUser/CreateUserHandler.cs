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

public sealed class CreateUserHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IClock clock,
    IValidator<CreateUserCommand> validator)
    : IRequestHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var actor = await ApplicationAccess.GetUserAsync(unitOfWork, request.ActorUserId, cancellationToken);
        try
        {
            UserProvisioningPolicy.EnsureCanCreate(actor, request.Role, request.SupportCategoryIds);
        }
        catch (DomainException exception) when (exception.Code is
            "ACTOR_INACTIVE" or
            "USER_CREATION_FORBIDDEN" or
            "USER_CREATION_OUTSIDE_SCOPE")
        {
            throw new UnauthorizedAccessException(exception.Message, exception);
        }

        if (await unitOfWork.Users.ExistsByEmailAsync(request.Email, null, cancellationToken))
        {
            throw new InvalidOperationException(ApplicationMessages.UserEmailAlreadyExists);
        }

        foreach (var categoryId in request.SupportCategoryIds.Distinct())
        {
            var category = await unitOfWork.SupportCategories.GetByIdAsync(categoryId, cancellationToken)
                ?? throw new KeyNotFoundException(
                    ApplicationMessages.AssignedSupportCategoryNotFound);
            if (!category.IsActive)
            {
                throw new DomainException(
                    ApplicationErrorCodes.CategoryInactive,
                    ApplicationMessages.InactiveSupportCategoryCannotBeAssigned);
            }
        }

        var passwordHash = passwordHasher.Hash(request.Password);
        var now = clock.UtcNow;
        var user = request.Role switch
        {
            UserRole.User => User.CreateUser(request.FullName, request.Email, passwordHash, now),
            UserRole.Technician => User.CreateTechnician(
                request.FullName,
                request.Email,
                passwordHash,
                request.SupportCategoryIds,
                request.MaxActiveTickets!.Value,
                now),
            UserRole.Supervisor => User.CreateSupervisor(
                request.FullName,
                request.Email,
                passwordHash,
                request.SupportCategoryIds.Single(),
                now),
            UserRole.SuperAdmin => User.CreateSuperAdmin(request.FullName, request.Email, passwordHash, now),
            _ => throw new DomainException(
                ApplicationErrorCodes.InvalidUserRole,
                ApplicationMessages.InvalidUserRole)
        };

        await unitOfWork.Users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}

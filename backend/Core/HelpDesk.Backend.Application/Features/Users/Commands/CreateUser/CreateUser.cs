using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Policies;
using HelpDesk.Backend.Domain.Users;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    Guid ActorUserId,
    string FullName,
    string Email,
    string Password,
    UserRole Role,
    IReadOnlyCollection<Guid> SupportCategoryIds,
    int? MaxActiveTickets) : IRequest<Guid>;

public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty().WithErrorCode("REQUIRED");
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Email).NotEmpty().MaximumLength(254).EmailAddress();
        RuleFor(command => command.Password).NotEmpty();
        RuleFor(command => command.Role).IsInEnum();
        RuleFor(command => command.SupportCategoryIds).NotNull();
        RuleForEach(command => command.SupportCategoryIds).NotEmpty();

        RuleFor(command => command.SupportCategoryIds)
            .Must((command, categories) => HasValidProfile(command.Role, categories, command.MaxActiveTickets))
            .WithErrorCode("INVALID_USER_PROFILE")
            .WithMessage("Las categorías y la capacidad no corresponden al rol solicitado.");
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
            throw new InvalidOperationException("Ya existe un usuario con el email indicado.");
        }

        foreach (var categoryId in request.SupportCategoryIds.Distinct())
        {
            var category = await unitOfWork.SupportCategories.GetByIdAsync(categoryId, cancellationToken)
                ?? throw new KeyNotFoundException("No se encontró una categoría asignada al usuario.");
            if (!category.IsActive)
            {
                throw new DomainException("CATEGORY_INACTIVE", "No se puede asignar una categoría inactiva.");
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
            _ => throw new DomainException("INVALID_USER_ROLE", "El rol indicado no es válido.")
        };

        await unitOfWork.Users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}

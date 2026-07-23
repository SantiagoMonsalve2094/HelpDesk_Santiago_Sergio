using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.ChangeSupervisorCategory;

public sealed record ChangeSupervisorCategoryCommand(
    Guid ActorUserId,
    Guid SupervisorUserId,
    Guid SupportCategoryId) : IRequest;

public sealed class ChangeSupervisorCategoryValidator : AbstractValidator<ChangeSupervisorCategoryCommand>
{
    public ChangeSupervisorCategoryValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.SupervisorUserId).NotEmpty();
        RuleFor(command => command.SupportCategoryId).NotEmpty();
    }
}

public sealed class ChangeSupervisorCategoryHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<ChangeSupervisorCategoryCommand> validator)
    : IRequestHandler<ChangeSupervisorCategoryCommand>
{
    public async Task Handle(ChangeSupervisorCategoryCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(unitOfWork, request.ActorUserId, cancellationToken);
        ApplicationAccess.EnsureSuperAdmin(actor);
        var supervisor = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.SupervisorUserId,
            cancellationToken);

        if (supervisor.Role != UserRole.Supervisor)
        {
            throw new DomainException("USER_IS_NOT_SUPERVISOR", "El usuario no tiene perfil de supervisor.");
        }

        var category = await unitOfWork.SupportCategories.GetByIdAsync(
            request.SupportCategoryId,
            cancellationToken)
            ?? throw new KeyNotFoundException("No se encontró la categoría solicitada.");
        if (!category.IsActive)
        {
            throw new DomainException("CATEGORY_INACTIVE", "No se puede asignar una categoría inactiva.");
        }

        supervisor.ChangeSupervisorCategory(category.Id, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

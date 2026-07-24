using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.Resources;
using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.ChangeSupervisorCategory;

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
            throw new DomainException(
                ApplicationErrorCodes.UserIsNotSupervisor,
                ApplicationMessages.UserIsNotSupervisor);
        }

        var category = await unitOfWork.SupportCategories.GetByIdAsync(
            request.SupportCategoryId,
            cancellationToken)
            ?? throw new KeyNotFoundException(ApplicationMessages.SupportCategoryNotFound);
        if (!category.IsActive)
        {
            throw new DomainException(
                ApplicationErrorCodes.CategoryInactive,
                ApplicationMessages.InactiveSupportCategoryCannotBeAssigned);
        }

        supervisor.ChangeSupervisorCategory(category.Id, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

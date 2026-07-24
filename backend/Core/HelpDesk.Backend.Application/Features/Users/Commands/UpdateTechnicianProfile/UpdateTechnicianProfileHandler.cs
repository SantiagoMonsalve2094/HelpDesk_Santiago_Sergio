using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.Resources;
using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.UpdateTechnicianProfile;

public sealed class UpdateTechnicianProfileHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<UpdateTechnicianProfileCommand> validator)
    : IRequestHandler<UpdateTechnicianProfileCommand>
{
    public async Task Handle(UpdateTechnicianProfileCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(unitOfWork, request.ActorUserId, cancellationToken);
        ApplicationAccess.EnsureSuperAdmin(actor);
        var technician = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.TechnicianUserId,
            cancellationToken);

        if (technician.Role != UserRole.Technician || technician.TechnicianProfile is null)
        {
            throw new DomainException(
                ApplicationErrorCodes.UserIsNotTechnician,
                ApplicationMessages.UserIsNotTechnician);
        }

        var requestedCategoryIds = request.SupportCategoryIds.Distinct().ToHashSet();
        foreach (var categoryId in requestedCategoryIds)
        {
            var category = await unitOfWork.SupportCategories.GetByIdAsync(categoryId, cancellationToken)
                ?? throw new KeyNotFoundException(ApplicationMessages.SupportCategoryNotFound);
            if (!category.IsActive)
            {
                throw new DomainException(
                    ApplicationErrorCodes.CategoryInactive,
                    ApplicationMessages.InactiveSupportCategoryCannotBeAssigned);
            }
        }

        var currentCategoryIds = technician.TechnicianProfile.SupportCategoryIds.ToHashSet();
        var now = clock.UtcNow;

        foreach (var categoryId in requestedCategoryIds.Except(currentCategoryIds))
        {
            technician.AddTechnicianCategory(categoryId, now);
        }

        foreach (var categoryId in currentCategoryIds.Except(requestedCategoryIds))
        {
            technician.RemoveTechnicianCategory(categoryId, now);
        }

        technician.ChangeTechnicianCapacity(request.MaxActiveTickets, now);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.UpdateTechnicianProfile;

public sealed record UpdateTechnicianProfileCommand(
    Guid ActorUserId,
    Guid TechnicianUserId,
    IReadOnlyCollection<Guid> SupportCategoryIds,
    int MaxActiveTickets) : IRequest;

public sealed class UpdateTechnicianProfileValidator : AbstractValidator<UpdateTechnicianProfileCommand>
{
    public UpdateTechnicianProfileValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.TechnicianUserId).NotEmpty();
        RuleFor(command => command.SupportCategoryIds).NotEmpty();
        RuleForEach(command => command.SupportCategoryIds).NotEmpty();
        RuleFor(command => command.MaxActiveTickets).GreaterThan(0);
    }
}

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
            throw new DomainException("USER_IS_NOT_TECHNICIAN", "El usuario no tiene perfil de técnico.");
        }

        var requestedCategoryIds = request.SupportCategoryIds.Distinct().ToHashSet();
        foreach (var categoryId in requestedCategoryIds)
        {
            var category = await unitOfWork.SupportCategories.GetByIdAsync(categoryId, cancellationToken)
                ?? throw new KeyNotFoundException("No se encontró una categoría solicitada.");
            if (!category.IsActive)
            {
                throw new DomainException("CATEGORY_INACTIVE", "No se puede asignar una categoría inactiva.");
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

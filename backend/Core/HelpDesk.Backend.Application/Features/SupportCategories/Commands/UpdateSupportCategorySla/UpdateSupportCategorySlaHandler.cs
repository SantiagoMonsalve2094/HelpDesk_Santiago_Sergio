using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.Resources;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Commands.UpdateSupportCategorySla;

public sealed class UpdateSupportCategorySlaHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<UpdateSupportCategorySlaCommand> validator)
    : IRequestHandler<UpdateSupportCategorySlaCommand>
{
    public async Task Handle(
        UpdateSupportCategorySlaCommand request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.ActorUserId,
            cancellationToken);

        if (!SupportCategoryAuthorizationPolicy.CanConfigureSla(actor, request.SupportCategoryId))
        {
            throw new UnauthorizedAccessException(ApplicationMessages.CannotModifyCategorySla);
        }

        var category = await ApplicationAccess.GetSupportCategoryAsync(
            unitOfWork,
            request.SupportCategoryId,
            cancellationToken);
        category.UpdateSla(request.Priority, request.ResponseTime, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

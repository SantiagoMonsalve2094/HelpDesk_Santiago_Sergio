using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.Resources;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Commands.UpdateSupportCategory;

public sealed class UpdateSupportCategoryHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<UpdateSupportCategoryCommand> validator)
    : IRequestHandler<UpdateSupportCategoryCommand>
{
    public async Task Handle(
        UpdateSupportCategoryCommand request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.ActorUserId,
            cancellationToken);
        ApplicationAccess.EnsureSuperAdmin(actor);

        var category = await ApplicationAccess.GetSupportCategoryAsync(
            unitOfWork,
            request.SupportCategoryId,
            cancellationToken);

        if (await unitOfWork.SupportCategories.ExistsByNameAsync(
                request.Name,
                category.Id,
                cancellationToken))
        {
            throw new InvalidOperationException(
                ApplicationMessages.SupportCategoryNameAlreadyExists);
        }

        category.UpdateInformation(request.Name, request.Description, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

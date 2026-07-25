using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.Resources;
using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Commands.CreateSupportCategory;

public sealed class CreateSupportCategoryHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<CreateSupportCategoryCommand> validator)
    : IRequestHandler<CreateSupportCategoryCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateSupportCategoryCommand request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.ActorUserId,
            cancellationToken);

        if (!SupportCategoryAuthorizationPolicy.CanCreateCategory(actor))
        {
            throw new UnauthorizedAccessException(
                ApplicationMessages.OnlyActiveSuperAdminCanCreateCategories);
        }

        if (await unitOfWork.SupportCategories.ExistsByNameAsync(
                request.Name,
                null,
                cancellationToken))
        {
            throw new InvalidOperationException(
                ApplicationMessages.SupportCategoryNameAlreadyExists);
        }

        var category = SupportCategory.Create(
            request.Name,
            request.Description,
            new Dictionary<TicketPriority, TimeSpan>
            {
                [TicketPriority.Low] = request.LowSla,
                [TicketPriority.Medium] = request.MediumSla,
                [TicketPriority.High] = request.HighSla,
                [TicketPriority.Critical] = request.CriticalSla
            },
            clock.UtcNow);

        await unitOfWork.SupportCategories.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}

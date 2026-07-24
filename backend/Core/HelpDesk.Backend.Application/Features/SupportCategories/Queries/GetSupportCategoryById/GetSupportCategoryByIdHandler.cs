using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.Resources;
using HelpDesk.Backend.Application.DTOs.SupportCategories;
using HelpDesk.Backend.Application.Features.SupportCategories;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Queries.GetSupportCategoryById;

public sealed class GetSupportCategoryByIdHandler(
    IUnitOfWork unitOfWork,
    IValidator<GetSupportCategoryByIdQuery> validator)
    : IRequestHandler<GetSupportCategoryByIdQuery, SupportCategoryDetailsResponse>
{
    public async Task<SupportCategoryDetailsResponse> Handle(
        GetSupportCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.ActorUserId,
            cancellationToken);
        if (!actor.IsActive)
        {
            throw new UnauthorizedAccessException(ApplicationMessages.ActiveUserRequired);
        }

        var category = await ApplicationAccess.GetSupportCategoryAsync(
            unitOfWork,
            request.SupportCategoryId,
            cancellationToken);
        if (!category.IsActive && actor.Role != UserRole.SuperAdmin)
        {
            throw new UnauthorizedAccessException(
                ApplicationMessages.OnlySuperAdminCanViewInactiveCategories);
        }

        return SupportCategoryMapper.ToDetails(category);
    }
}

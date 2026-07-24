using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.Resources;
using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.Common.Validation;
using HelpDesk.Backend.Application.DTOs.SupportCategories;
using HelpDesk.Backend.Application.Features.SupportCategories;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Queries.GetSupportCategories;

public sealed class GetSupportCategoriesHandler(
    IUnitOfWork unitOfWork,
    ISupportCategoryReadRepository readRepository,
    IValidator<GetSupportCategoriesQuery> validator)
    : IRequestHandler<GetSupportCategoriesQuery, PagedResponse<SupportCategorySummaryResponse>>
{
    public async Task<PagedResponse<SupportCategorySummaryResponse>> Handle(
        GetSupportCategoriesQuery request,
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

        if (request.IncludeInactive && actor.Role != UserRole.SuperAdmin)
        {
            throw new UnauthorizedAccessException(
                ApplicationMessages.OnlySuperAdminCanViewInactiveCategories);
        }

        return await readRepository.GetPagedAsync(
            new SupportCategoryReadFilter(request.IncludeInactive),
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}

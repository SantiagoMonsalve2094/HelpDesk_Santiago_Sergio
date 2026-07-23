using FluentValidation;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Abstractions.Queries;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Application.Common.Models;
using HelpDesk.Backend.Application.Common.Validation;
using HelpDesk.Backend.Application.Features.SupportCategories.Models;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Queries.GetSupportCategories;

public sealed record GetSupportCategoriesQuery(
    Guid ActorUserId,
    bool IncludeInactive = false,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResponse<SupportCategorySummaryResponse>>;

public sealed class GetSupportCategoriesValidator : AbstractValidator<GetSupportCategoriesQuery>
{
    public GetSupportCategoriesValidator()
    {
        RuleFor(query => query.ActorUserId).NotEmpty();
        this.ApplyPaginationRules(query => query.PageNumber, query => query.PageSize);
    }
}

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
            throw new UnauthorizedAccessException("La operación requiere un usuario activo.");
        }

        if (request.IncludeInactive && actor.Role != UserRole.SuperAdmin)
        {
            throw new UnauthorizedAccessException(
                "Solo un SuperAdmin puede consultar categorías inactivas.");
        }

        return await readRepository.GetPagedAsync(
            new SupportCategoryReadFilter(request.IncludeInactive),
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}

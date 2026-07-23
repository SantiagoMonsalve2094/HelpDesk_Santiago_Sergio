using FluentValidation;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Application.Features.SupportCategories.Models;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Queries.GetSupportCategoryById;

public sealed record GetSupportCategoryByIdQuery(
    Guid ActorUserId,
    Guid SupportCategoryId) : IRequest<SupportCategoryDetailsResponse>;

public sealed class GetSupportCategoryByIdValidator : AbstractValidator<GetSupportCategoryByIdQuery>
{
    public GetSupportCategoryByIdValidator()
    {
        RuleFor(query => query.ActorUserId).NotEmpty();
        RuleFor(query => query.SupportCategoryId).NotEmpty();
    }
}

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
            throw new UnauthorizedAccessException("La operación requiere un usuario activo.");
        }

        var category = await ApplicationAccess.GetSupportCategoryAsync(
            unitOfWork,
            request.SupportCategoryId,
            cancellationToken);
        if (!category.IsActive && actor.Role != UserRole.SuperAdmin)
        {
            throw new UnauthorizedAccessException(
                "Solo un SuperAdmin puede consultar categorías inactivas.");
        }

        return SupportCategoryMapper.ToDetails(category);
    }
}

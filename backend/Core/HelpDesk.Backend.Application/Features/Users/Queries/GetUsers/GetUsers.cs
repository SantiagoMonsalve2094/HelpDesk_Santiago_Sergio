using FluentValidation;
using HelpDesk.Backend.Application.Abstractions.Queries;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Application.Common.Models;
using HelpDesk.Backend.Application.Common.Validation;
using HelpDesk.Backend.Application.Features.Users.Models;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Queries.GetUsers;

public sealed record GetUsersQuery(
    Guid ActorUserId,
    UserRole? Role = null,
    Guid? SupportCategoryId = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResponse<UserSummaryResponse>>;

public sealed class GetUsersValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersValidator()
    {
        RuleFor(query => query.ActorUserId).NotEmpty();
        RuleFor(query => query.Role).IsInEnum().When(query => query.Role.HasValue);
        this.ApplyPaginationRules(query => query.PageNumber, query => query.PageSize);
    }
}

public sealed class GetUsersHandler(
    IUnitOfWork unitOfWork,
    IUserReadRepository readRepository,
    IValidator<GetUsersQuery> validator)
    : IRequestHandler<GetUsersQuery, PagedResponse<UserSummaryResponse>>
{
    public async Task<PagedResponse<UserSummaryResponse>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(unitOfWork, request.ActorUserId, cancellationToken);
        ApplicationAccess.EnsureSuperAdmin(actor);

        return await readRepository.GetPagedAsync(
            new UserReadFilter(request.Role, request.SupportCategoryId, request.IsActive),
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}

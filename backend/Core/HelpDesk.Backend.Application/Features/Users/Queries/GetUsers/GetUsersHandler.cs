using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.Common.Validation;
using HelpDesk.Backend.Application.DTOs.Users;
using HelpDesk.Backend.Application.Features.Users;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Queries.GetUsers;

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

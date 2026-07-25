using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.Resources;
using HelpDesk.Backend.Application.DTOs.Auth;
using HelpDesk.Backend.Application.Features.Auth;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserHandler(
    IUnitOfWork unitOfWork,
    IValidator<GetCurrentUserQuery> validator)
    : IRequestHandler<GetCurrentUserQuery, AuthenticatedUserResponse>
{
    public async Task<AuthenticatedUserResponse> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var user = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.ActorUserId,
            cancellationToken);

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(ApplicationMessages.UserInactive);
        }

        return AuthUserMapper.ToResponse(user);
    }
}

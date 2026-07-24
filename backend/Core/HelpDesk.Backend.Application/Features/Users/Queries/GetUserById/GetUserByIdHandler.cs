using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Users;
using HelpDesk.Backend.Application.Features.Users;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdHandler(
    IUnitOfWork unitOfWork,
    IValidator<GetUserByIdQuery> validator)
    : IRequestHandler<GetUserByIdQuery, UserDetailsResponse>
{
    public async Task<UserDetailsResponse> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(unitOfWork, request.ActorUserId, cancellationToken);
        ApplicationAccess.EnsureSuperAdmin(actor);
        var user = await ApplicationAccess.GetUserAsync(unitOfWork, request.UserId, cancellationToken);
        return UserMapper.ToDetails(user);
    }
}

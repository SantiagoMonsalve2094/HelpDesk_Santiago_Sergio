using FluentValidation;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Application.Features.Auth.Models;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery(
    Guid ActorUserId) : IRequest<AuthenticatedUserResponse>;

public sealed class GetCurrentUserValidator : AbstractValidator<GetCurrentUserQuery>
{
    public GetCurrentUserValidator()
    {
        RuleFor(query => query.ActorUserId).NotEmpty();
    }
}

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
            throw new UnauthorizedAccessException("El usuario está inactivo.");
        }

        return AuthUserMapper.ToResponse(user);
    }
}

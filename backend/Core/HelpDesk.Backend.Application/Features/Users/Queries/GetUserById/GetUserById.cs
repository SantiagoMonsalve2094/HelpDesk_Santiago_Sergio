using FluentValidation;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Application.Features.Users.Models;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid ActorUserId, Guid UserId) : IRequest<UserDetailsResponse>;

public sealed class GetUserByIdValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdValidator()
    {
        RuleFor(query => query.ActorUserId).NotEmpty();
        RuleFor(query => query.UserId).NotEmpty();
    }
}

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

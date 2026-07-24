using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.ResetUserPassword;

public sealed class ResetUserPasswordHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IClock clock,
    IValidator<ResetUserPasswordCommand> validator)
    : IRequestHandler<ResetUserPasswordCommand>
{
    public async Task Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(unitOfWork, request.ActorUserId, cancellationToken);
        ApplicationAccess.EnsureSuperAdmin(actor);
        var user = await ApplicationAccess.GetUserAsync(unitOfWork, request.UserId, cancellationToken);

        user.ChangePasswordHash(passwordHasher.Hash(request.Password), clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

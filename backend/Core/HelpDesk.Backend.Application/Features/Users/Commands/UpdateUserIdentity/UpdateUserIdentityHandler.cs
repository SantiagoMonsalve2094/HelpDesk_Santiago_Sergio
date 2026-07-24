using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.Resources;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.UpdateUserIdentity;

public sealed class UpdateUserIdentityHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<UpdateUserIdentityCommand> validator)
    : IRequestHandler<UpdateUserIdentityCommand>
{
    public async Task Handle(UpdateUserIdentityCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(unitOfWork, request.ActorUserId, cancellationToken);
        ApplicationAccess.EnsureSuperAdmin(actor);
        var user = await ApplicationAccess.GetUserAsync(unitOfWork, request.UserId, cancellationToken);

        if (await unitOfWork.Users.ExistsByEmailAsync(request.Email, user.Id, cancellationToken))
        {
            throw new InvalidOperationException(ApplicationMessages.UserEmailAlreadyExists);
        }

        user.UpdateIdentity(request.FullName, request.Email, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

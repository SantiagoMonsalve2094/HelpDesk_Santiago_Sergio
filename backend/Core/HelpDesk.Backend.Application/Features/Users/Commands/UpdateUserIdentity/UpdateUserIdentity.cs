using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.UpdateUserIdentity;

public sealed record UpdateUserIdentityCommand(
    Guid ActorUserId,
    Guid UserId,
    string FullName,
    string Email) : IRequest;

public sealed class UpdateUserIdentityValidator : AbstractValidator<UpdateUserIdentityCommand>
{
    public UpdateUserIdentityValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Email).NotEmpty().MaximumLength(254).EmailAddress();
    }
}

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
            throw new InvalidOperationException("Ya existe un usuario con el email indicado.");
        }

        user.UpdateIdentity(request.FullName, request.Email, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

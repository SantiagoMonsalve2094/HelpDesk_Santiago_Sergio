using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.ResetUserPassword;

public sealed record ResetUserPasswordCommand(
    Guid ActorUserId,
    Guid UserId,
    string Password) : IRequest;

public sealed class ResetUserPasswordValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Password).NotEmpty();
    }
}

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

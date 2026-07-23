using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.SetUserActive;

public sealed record SetUserActiveCommand(
    Guid ActorUserId,
    Guid UserId,
    bool IsActive) : IRequest;

public sealed class SetUserActiveValidator : AbstractValidator<SetUserActiveCommand>
{
    public SetUserActiveValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
    }
}

public sealed class SetUserActiveHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<SetUserActiveCommand> validator)
    : IRequestHandler<SetUserActiveCommand>
{
    public async Task Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(unitOfWork, request.ActorUserId, cancellationToken);
        ApplicationAccess.EnsureSuperAdmin(actor);
        var user = await ApplicationAccess.GetUserAsync(unitOfWork, request.UserId, cancellationToken);

        if (request.IsActive)
        {
            user.Activate(clock.UtcNow);
        }
        else
        {
            user.Deactivate(clock.UtcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Commands.SetSupportCategoryActive;

public sealed record SetSupportCategoryActiveCommand(
    Guid ActorUserId,
    Guid SupportCategoryId,
    bool IsActive) : IRequest;

public sealed class SetSupportCategoryActiveValidator : AbstractValidator<SetSupportCategoryActiveCommand>
{
    public SetSupportCategoryActiveValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.SupportCategoryId).NotEmpty();
    }
}

public sealed class SetSupportCategoryActiveHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<SetSupportCategoryActiveCommand> validator)
    : IRequestHandler<SetSupportCategoryActiveCommand>
{
    public async Task Handle(
        SetSupportCategoryActiveCommand request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.ActorUserId,
            cancellationToken);
        ApplicationAccess.EnsureSuperAdmin(actor);

        var category = await ApplicationAccess.GetSupportCategoryAsync(
            unitOfWork,
            request.SupportCategoryId,
            cancellationToken);

        if (request.IsActive)
        {
            category.Activate(clock.UtcNow);
        }
        else
        {
            category.Deactivate(clock.UtcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

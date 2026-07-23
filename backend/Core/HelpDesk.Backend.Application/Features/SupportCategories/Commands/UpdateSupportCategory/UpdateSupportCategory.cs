using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Commands.UpdateSupportCategory;

public sealed record UpdateSupportCategoryCommand(
    Guid ActorUserId,
    Guid SupportCategoryId,
    string Name,
    string Description) : IRequest;

public sealed class UpdateSupportCategoryValidator : AbstractValidator<UpdateSupportCategoryCommand>
{
    public UpdateSupportCategoryValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.SupportCategoryId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Description).NotEmpty().MaximumLength(1000);
    }
}

public sealed class UpdateSupportCategoryHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<UpdateSupportCategoryCommand> validator)
    : IRequestHandler<UpdateSupportCategoryCommand>
{
    public async Task Handle(
        UpdateSupportCategoryCommand request,
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

        if (await unitOfWork.SupportCategories.ExistsByNameAsync(
                request.Name,
                category.Id,
                cancellationToken))
        {
            throw new InvalidOperationException("Ya existe una categoría con el nombre indicado.");
        }

        category.UpdateInformation(request.Name, request.Description, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

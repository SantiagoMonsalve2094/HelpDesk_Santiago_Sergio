using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Commands.UpdateCategorySla;

public sealed record UpdateCategorySlaCommand(
    Guid ActorUserId,
    Guid SupportCategoryId,
    TicketPriority Priority,
    TimeSpan ResponseTime) : IRequest;

public sealed class UpdateCategorySlaValidator : AbstractValidator<UpdateCategorySlaCommand>
{
    public UpdateCategorySlaValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.SupportCategoryId).NotEmpty();
        RuleFor(command => command.Priority).IsInEnum();
        RuleFor(command => command.ResponseTime).GreaterThan(TimeSpan.Zero);
    }
}

public sealed class UpdateCategorySlaHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<UpdateCategorySlaCommand> validator)
    : IRequestHandler<UpdateCategorySlaCommand>
{
    public async Task Handle(
        UpdateCategorySlaCommand request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.ActorUserId,
            cancellationToken);

        if (!SupportCategoryAuthorizationPolicy.CanConfigureSla(actor, request.SupportCategoryId))
        {
            throw new UnauthorizedAccessException(
                "El usuario no puede modificar el SLA de esta categoría.");
        }

        var category = await ApplicationAccess.GetSupportCategoryAsync(
            unitOfWork,
            request.SupportCategoryId,
            cancellationToken);
        category.UpdateSla(request.Priority, request.ResponseTime, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

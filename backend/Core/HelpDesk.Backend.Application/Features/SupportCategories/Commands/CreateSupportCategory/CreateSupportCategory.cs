using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Domain.Categories;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Commands.CreateSupportCategory;

public sealed record CreateSupportCategoryCommand(
    Guid ActorUserId,
    string Name,
    string Description,
    TimeSpan LowSla,
    TimeSpan MediumSla,
    TimeSpan HighSla,
    TimeSpan CriticalSla) : IRequest<Guid>;

public sealed class CreateSupportCategoryValidator : AbstractValidator<CreateSupportCategoryCommand>
{
    public CreateSupportCategoryValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Description).NotEmpty().MaximumLength(1000);
        RuleFor(command => command.LowSla).GreaterThan(TimeSpan.Zero);
        RuleFor(command => command.MediumSla).GreaterThan(TimeSpan.Zero);
        RuleFor(command => command.HighSla).GreaterThan(TimeSpan.Zero);
        RuleFor(command => command.CriticalSla).GreaterThan(TimeSpan.Zero);
    }
}

public sealed class CreateSupportCategoryHandler(
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<CreateSupportCategoryCommand> validator)
    : IRequestHandler<CreateSupportCategoryCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateSupportCategoryCommand request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.ActorUserId,
            cancellationToken);

        if (!SupportCategoryAuthorizationPolicy.CanCreateCategory(actor))
        {
            throw new UnauthorizedAccessException("Solo un SuperAdmin activo puede crear categorías.");
        }

        if (await unitOfWork.SupportCategories.ExistsByNameAsync(
                request.Name,
                null,
                cancellationToken))
        {
            throw new InvalidOperationException("Ya existe una categoría con el nombre indicado.");
        }

        var category = SupportCategory.Create(
            request.Name,
            request.Description,
            new Dictionary<TicketPriority, TimeSpan>
            {
                [TicketPriority.Low] = request.LowSla,
                [TicketPriority.Medium] = request.MediumSla,
                [TicketPriority.High] = request.HighSla,
                [TicketPriority.Critical] = request.CriticalSla
            },
            clock.UtcNow);

        await unitOfWork.SupportCategories.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}

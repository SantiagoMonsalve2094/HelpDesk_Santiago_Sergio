using FluentValidation;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Abstractions.Queries;
using HelpDesk.Backend.Application.Common;
using HelpDesk.Backend.Application.Features.Tickets.Models;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Queries.GetSlaReport;

public sealed record GetSlaReportQuery(
    Guid ActorUserId,
    Guid? SupportCategoryId = null,
    Guid? TechnicianUserId = null,
    DateTimeOffset? FromUtc = null,
    DateTimeOffset? ToUtc = null) : IRequest<SlaReportResponse>;

public sealed class GetSlaReportValidator : AbstractValidator<GetSlaReportQuery>
{
    public GetSlaReportValidator()
    {
        RuleFor(query => query.ActorUserId).NotEmpty();
        RuleFor(query => query)
            .Must(query =>
                query.FromUtc is null ||
                query.ToUtc is null ||
                query.FromUtc <= query.ToUtc)
            .WithMessage("La fecha inicial no puede ser posterior a la fecha final.");
    }
}

public sealed class GetSlaReportHandler(
    IUnitOfWork unitOfWork,
    ISlaReportReadRepository readRepository,
    IValidator<GetSlaReportQuery> validator)
    : IRequestHandler<GetSlaReportQuery, SlaReportResponse>
{
    public async Task<SlaReportResponse> Handle(
        GetSlaReportQuery request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ApplicationAccess.GetUserAsync(
            unitOfWork,
            request.ActorUserId,
            cancellationToken);

        Guid? categoryId;
        if (actor.IsActive && actor.Role == UserRole.SuperAdmin)
        {
            categoryId = request.SupportCategoryId;
        }
        else if (actor.IsActive &&
                 actor.Role == UserRole.Supervisor &&
                 actor.SupervisorProfile is not null)
        {
            categoryId = actor.SupervisorProfile.SupportCategoryId;
            if (request.SupportCategoryId is Guid requestedCategoryId &&
                requestedCategoryId != categoryId)
            {
                throw new UnauthorizedAccessException(
                    "El supervisor solo puede consultar el reporte de su categoría.");
            }
        }
        else
        {
            throw new UnauthorizedAccessException(
                "El reporte SLA requiere un Supervisor o SuperAdmin activo.");
        }

        return await readRepository.GetReportAsync(
            new SlaReportFilter(
                categoryId,
                request.TechnicianUserId,
                request.FromUtc,
                request.ToUtc),
            cancellationToken);
    }
}

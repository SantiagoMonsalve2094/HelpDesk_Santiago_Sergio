using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.Resources;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Sla.Queries.GetSlaReport;

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
                    ApplicationMessages.SupervisorCanOnlyViewOwnCategoryReport);
            }
        }
        else
        {
            throw new UnauthorizedAccessException(
                ApplicationMessages.SlaReportRequiresSupervisorOrSuperAdmin);
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

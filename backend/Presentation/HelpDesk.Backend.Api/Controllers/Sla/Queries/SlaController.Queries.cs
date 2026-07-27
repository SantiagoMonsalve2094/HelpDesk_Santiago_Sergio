using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.Features.Sla.Queries.GetSlaAlerts;
using HelpDesk.Backend.Application.Features.Sla.Queries.GetSlaReport;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Backend.Api.Controllers;

public sealed partial class SlaController
{
    [HttpGet("alerts")]
    public async Task<ActionResult<PagedResponse<SlaAlertResponse>>> GetAlerts(
        Guid? supportCategoryId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new GetSlaAlertsQuery(
                ActorUserId,
                supportCategoryId,
                pageNumber,
                pageSize),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("report")]
    public async Task<ActionResult<SlaReportResponse>> GetReport(
        Guid? supportCategoryId,
        Guid? technicianUserId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new GetSlaReportQuery(
                ActorUserId,
                supportCategoryId,
                technicianUserId,
                fromUtc,
                toUtc),
            cancellationToken);

        return Ok(response);
    }
}

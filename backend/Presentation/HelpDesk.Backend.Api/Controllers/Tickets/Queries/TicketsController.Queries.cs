using HelpDesk.Backend.Api.Security;
using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets.Queries.GetAssignableTechnicians;
using HelpDesk.Backend.Application.Features.Tickets.Queries.GetTicketById;
using HelpDesk.Backend.Application.Features.Tickets.Queries.GetTickets;
using HelpDesk.Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Backend.Api.Controllers;

public sealed partial class TicketsController
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<TicketSummaryResponse>>> Get(
        TicketStatus? status,
        TicketPriority? priority,
        Guid? supportCategoryId,
        Guid? technicianUserId,
        bool? isOverdue,
        DateTimeOffset? createdFromUtc,
        DateTimeOffset? createdToUtc,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new GetTicketsQuery(
                ActorUserId,
                status,
                priority,
                supportCategoryId,
                technicianUserId,
                isOverdue,
                createdFromUtc,
                createdToUtc,
                pageNumber,
                pageSize),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketDetailsResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetTicketByIdQuery(ActorUserId, id),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:guid}/assignable-technicians")]
    [Authorize(Roles = RoleNames.SupervisorOrSuperAdmin)]
    public async Task<ActionResult<IReadOnlyList<AssignableTechnicianResponse>>>
        GetAssignableTechnicians(
            Guid id,
            CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetAssignableTechniciansQuery(ActorUserId, id),
            cancellationToken);

        return Ok(response);
    }
}

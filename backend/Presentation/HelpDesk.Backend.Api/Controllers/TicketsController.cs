using HelpDesk.Backend.Api.Security;
using HelpDesk.Backend.Api.DTOs.Tickets;
using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.Features.Tickets.Commands.AddTicketComment;
using HelpDesk.Backend.Application.Features.Tickets.Commands.AssignTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.CloseTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.CreateTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.DeleteTicket;
<<<<<<< HEAD
=======
using HelpDesk.Backend.Application.Features.Tickets.Commands.ForceTicketStatus;
>>>>>>> 60bd3aa8c163527f2e018e15a29114b99aa06847
using HelpDesk.Backend.Application.Features.Tickets.Commands.ReassignTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.ReopenTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.ResolveTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.StartTicketProgress;
using HelpDesk.Backend.Application.Features.Tickets.Commands.UpdateTicket;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets.Queries.GetAssignableTechnicians;
using HelpDesk.Backend.Application.Features.Tickets.Queries.GetTicketById;
using HelpDesk.Backend.Application.Features.Tickets.Queries.GetTickets;
using HelpDesk.Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Backend.Api.Controllers;

[Route("api/tickets")]
public sealed class TicketsController(ISender sender) : ApiControllerBase
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
        var response = await sender.Send(
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
        var response = await sender.Send(
            new GetTicketByIdQuery(ActorUserId, id),
            cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CreatedTicketResponse>> Create(
        CreateTicketApiRequest request,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(
            new CreateTicketCommand(
                ActorUserId,
                request.Subject,
                request.Description,
                request.SupportCategoryId,
                request.Priority),
            cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = response.TicketId },
            response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateTicketApiRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new UpdateTicketCommand(
                ActorUserId,
                id,
                request.Subject,
                request.Description),
            cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new DeleteTicketCommand(ActorUserId, id),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/comments")]
    public async Task<IActionResult> Comment(
        Guid id,
        AddTicketCommentApiRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new AddTicketCommentCommand(ActorUserId, id, request.Text),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = RoleNames.SupervisorOrSuperAdmin)]
    public async Task<IActionResult> Assign(
        Guid id,
        AssignTicketApiRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new AssignTicketCommand(ActorUserId, id, request.TechnicianUserId),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reassign")]
    [Authorize(Roles = RoleNames.SupervisorOrSuperAdmin)]
    public async Task<IActionResult> Reassign(
        Guid id,
        ReassignTicketApiRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new ReassignTicketCommand(
                ActorUserId,
                id,
                request.TechnicianUserId,
                request.Reason),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/start")]
    [Authorize(Roles = RoleNames.TechnicianSupervisorOrSuperAdmin)]
    public async Task<IActionResult> Start(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new StartTicketProgressCommand(ActorUserId, id),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/resolve")]
    [Authorize(Roles = RoleNames.TechnicianSupervisorOrSuperAdmin)]
    public async Task<IActionResult> Resolve(
        Guid id,
        ResolveTicketApiRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new ResolveTicketCommand(
                ActorUserId,
                id,
                request.ResolutionComment),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new CloseTicketCommand(ActorUserId, id),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reopen")]
    public async Task<IActionResult> Reopen(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new ReopenTicketCommand(ActorUserId, id),
            cancellationToken);
        return NoContent();
    }

<<<<<<< HEAD
=======
    [HttpPost("{id:guid}/force-status")]
    [Authorize(Roles = RoleNames.SupervisorOrSuperAdmin)]
    public async Task<IActionResult> ForceStatus(
        Guid id,
        ForceTicketStatusApiRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new ForceTicketStatusCommand(
                ActorUserId,
                id,
                request.TargetStatus,
                request.Justification),
            cancellationToken);
        return NoContent();
    }

>>>>>>> 60bd3aa8c163527f2e018e15a29114b99aa06847
    [HttpGet("{id:guid}/assignable-technicians")]
    [Authorize(Roles = RoleNames.SupervisorOrSuperAdmin)]
    public async Task<ActionResult<IReadOnlyList<AssignableTechnicianResponse>>>
        GetAssignableTechnicians(
            Guid id,
            CancellationToken cancellationToken)
    {
        var response = await sender.Send(
            new GetAssignableTechniciansQuery(ActorUserId, id),
            cancellationToken);
        return Ok(response);
    }
}

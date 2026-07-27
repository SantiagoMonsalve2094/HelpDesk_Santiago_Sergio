using HelpDesk.Backend.Api.DTOs.Tickets;
using HelpDesk.Backend.Api.Security;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets.Commands.AddTicketComment;
using HelpDesk.Backend.Application.Features.Tickets.Commands.AssignTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.CloseTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.CreateTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.DeleteTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.ReassignTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.ReopenTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.ResolveTicket;
using HelpDesk.Backend.Application.Features.Tickets.Commands.StartTicketProgress;
using HelpDesk.Backend.Application.Features.Tickets.Commands.UpdateTicket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Backend.Api.Controllers;

public sealed partial class TicketsController
{
    [HttpPost]
    public async Task<ActionResult<CreatedTicketResponse>> Create(
        CreateTicketApiRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
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
        await _sender.Send(
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
        await _sender.Send(
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
        await _sender.Send(
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
        await _sender.Send(
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
        await _sender.Send(
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
        await _sender.Send(
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
        await _sender.Send(
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
        await _sender.Send(
            new CloseTicketCommand(ActorUserId, id),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/reopen")]
    public async Task<IActionResult> Reopen(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new ReopenTicketCommand(ActorUserId, id),
            cancellationToken);

        return NoContent();
    }
}

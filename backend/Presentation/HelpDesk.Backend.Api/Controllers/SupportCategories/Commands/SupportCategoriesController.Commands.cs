using HelpDesk.Backend.Api.DTOs.SupportCategories;
using HelpDesk.Backend.Api.Security;
using HelpDesk.Backend.Application.Features.SupportCategories.Commands.CreateSupportCategory;
using HelpDesk.Backend.Application.Features.SupportCategories.Commands.SetSupportCategoryActive;
using HelpDesk.Backend.Application.Features.SupportCategories.Commands.UpdateSupportCategory;
using HelpDesk.Backend.Application.Features.SupportCategories.Commands.UpdateSupportCategorySla;
using HelpDesk.Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Backend.Api.Controllers;

public sealed partial class SupportCategoriesController
{
    [HttpPost]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<Guid>> Create(
        CreateSupportCategoryApiRequest request,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(
            new CreateSupportCategoryCommand(
                ActorUserId,
                request.Name,
                request.Description,
                Minutes(request.LowSlaMinutes),
                Minutes(request.MediumSlaMinutes),
                Minutes(request.HighSlaMinutes),
                Minutes(request.CriticalSlaMinutes)),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateSupportCategoryApiRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new UpdateSupportCategoryCommand(
                ActorUserId,
                id,
                request.Name,
                request.Description),
            cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:guid}/sla/{priority}")]
    [Authorize(Roles = RoleNames.SupervisorOrSuperAdmin)]
    public async Task<IActionResult> UpdateSla(
        Guid id,
        TicketPriority priority,
        UpdateSupportCategorySlaApiRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new UpdateSupportCategorySlaCommand(
                ActorUserId,
                id,
                priority,
                Minutes(request.ResponseTimeMinutes)),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/active")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> SetActive(
        Guid id,
        SetSupportCategoryActiveApiRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new SetSupportCategoryActiveCommand(ActorUserId, id, request.IsActive),
            cancellationToken);

        return NoContent();
    }
}

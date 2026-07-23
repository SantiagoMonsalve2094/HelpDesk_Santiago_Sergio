using HelpDesk.Backend.Api.Authorization;
using HelpDesk.Backend.Api.Contracts;
using HelpDesk.Backend.Application.Common.Models;
using HelpDesk.Backend.Application.Features.SupportCategories.Commands.CreateSupportCategory;
using HelpDesk.Backend.Application.Features.SupportCategories.Commands.SetSupportCategoryActive;
using HelpDesk.Backend.Application.Features.SupportCategories.Commands.UpdateCategorySla;
using HelpDesk.Backend.Application.Features.SupportCategories.Commands.UpdateSupportCategory;
using HelpDesk.Backend.Application.Features.SupportCategories.Models;
using HelpDesk.Backend.Application.Features.SupportCategories.Queries.GetSupportCategories;
using HelpDesk.Backend.Application.Features.SupportCategories.Queries.GetSupportCategoryById;
using HelpDesk.Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Backend.Api.Controllers;

[Route("api/support-categories")]
public sealed class SupportCategoriesController(ISender sender) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<SupportCategorySummaryResponse>>> Get(
        bool includeInactive = false,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await sender.Send(
            new GetSupportCategoriesQuery(
                ActorUserId,
                includeInactive,
                pageNumber,
                pageSize),
            cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SupportCategoryDetailsResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(
            new GetSupportCategoryByIdQuery(ActorUserId, id),
            cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<Guid>> Create(
        CreateSupportCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(
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
        UpdateSupportCategoryRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
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
        UpdateCategorySlaRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new UpdateCategorySlaCommand(
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
        SetActiveRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new SetSupportCategoryActiveCommand(ActorUserId, id, request.IsActive),
            cancellationToken);
        return NoContent();
    }

    private static TimeSpan Minutes(int value) =>
        TimeSpan.FromMinutes(value);
}

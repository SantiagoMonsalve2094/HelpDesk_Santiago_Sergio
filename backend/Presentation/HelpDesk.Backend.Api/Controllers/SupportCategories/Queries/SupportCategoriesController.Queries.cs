using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.DTOs.SupportCategories;
using HelpDesk.Backend.Application.Features.SupportCategories.Queries.GetSupportCategories;
using HelpDesk.Backend.Application.Features.SupportCategories.Queries.GetSupportCategoryById;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Backend.Api.Controllers;

public sealed partial class SupportCategoriesController
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<SupportCategorySummaryResponse>>> Get(
        bool includeInactive = false,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
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
        var response = await _sender.Send(
            new GetSupportCategoryByIdQuery(ActorUserId, id),
            cancellationToken);

        return Ok(response);
    }
}

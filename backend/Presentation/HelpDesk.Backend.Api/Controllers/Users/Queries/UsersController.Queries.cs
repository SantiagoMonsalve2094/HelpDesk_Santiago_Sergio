using HelpDesk.Backend.Api.Security;
using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.DTOs.Users;
using HelpDesk.Backend.Application.Features.Users.Queries.GetUserById;
using HelpDesk.Backend.Application.Features.Users.Queries.GetUsers;
using HelpDesk.Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Backend.Api.Controllers;

public sealed partial class UsersController
{
    [HttpGet]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<PagedResponse<UserSummaryResponse>>> Get(
        UserRole? role,
        Guid? supportCategoryId,
        bool? isActive,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new GetUsersQuery(
                ActorUserId,
                role,
                supportCategoryId,
                isActive,
                pageNumber,
                pageSize),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<UserDetailsResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetUserByIdQuery(ActorUserId, id),
            cancellationToken);

        return Ok(response);
    }
}

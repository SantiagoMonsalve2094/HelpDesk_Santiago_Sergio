using HelpDesk.Backend.Api.Authorization;
using HelpDesk.Backend.Api.Contracts;
using HelpDesk.Backend.Application.Common.Models;
using HelpDesk.Backend.Application.Features.Users.Commands.ChangeSupervisorCategory;
using HelpDesk.Backend.Application.Features.Users.Commands.CreateUser;
using HelpDesk.Backend.Application.Features.Users.Commands.ResetUserPassword;
using HelpDesk.Backend.Application.Features.Users.Commands.SetUserActive;
using HelpDesk.Backend.Application.Features.Users.Commands.UpdateTechnicianProfile;
using HelpDesk.Backend.Application.Features.Users.Commands.UpdateUserIdentity;
using HelpDesk.Backend.Application.Features.Users.Models;
using HelpDesk.Backend.Application.Features.Users.Queries.GetUserById;
using HelpDesk.Backend.Application.Features.Users.Queries.GetUsers;
using HelpDesk.Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Backend.Api.Controllers;

[Route("api/users")]
public sealed class UsersController(ISender sender) : ApiControllerBase
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
        var response = await sender.Send(
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
        var response = await sender.Send(
            new GetUserByIdQuery(ActorUserId, id),
            cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.SupervisorOrSuperAdmin)]
    public async Task<ActionResult<Guid>> Create(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(
            new CreateUserCommand(
                ActorUserId,
                request.FullName,
                request.Email,
                request.Password,
                request.Role,
                request.SupportCategoryIds,
                request.MaxActiveTickets),
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}/identity")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> UpdateIdentity(
        Guid id,
        UpdateUserIdentityRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new UpdateUserIdentityCommand(
                ActorUserId,
                id,
                request.FullName,
                request.Email),
            cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/password")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> ResetPassword(
        Guid id,
        ResetUserPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new ResetUserPasswordCommand(ActorUserId, id, request.Password),
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
            new SetUserActiveCommand(ActorUserId, id, request.IsActive),
            cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/technician-profile")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> UpdateTechnicianProfile(
        Guid id,
        UpdateTechnicianProfileRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new UpdateTechnicianProfileCommand(
                ActorUserId,
                id,
                request.SupportCategoryIds,
                request.MaxActiveTickets),
            cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/supervisor-category")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> ChangeSupervisorCategory(
        Guid id,
        ChangeSupervisorCategoryRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new ChangeSupervisorCategoryCommand(
                ActorUserId,
                id,
                request.SupportCategoryId),
            cancellationToken);
        return NoContent();
    }
}

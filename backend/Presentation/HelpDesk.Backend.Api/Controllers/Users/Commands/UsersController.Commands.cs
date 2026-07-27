using HelpDesk.Backend.Api.DTOs.Users;
using HelpDesk.Backend.Api.Security;
using HelpDesk.Backend.Application.Features.Users.Commands.ChangeSupervisorCategory;
using HelpDesk.Backend.Application.Features.Users.Commands.CreateUser;
using HelpDesk.Backend.Application.Features.Users.Commands.ResetUserPassword;
using HelpDesk.Backend.Application.Features.Users.Commands.SetUserActive;
using HelpDesk.Backend.Application.Features.Users.Commands.UpdateTechnicianProfile;
using HelpDesk.Backend.Application.Features.Users.Commands.UpdateUserIdentity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Backend.Api.Controllers;

public sealed partial class UsersController
{
    [HttpPost]
    [Authorize(Roles = RoleNames.SupervisorOrSuperAdmin)]
    public async Task<ActionResult<Guid>> Create(
        CreateUserApiRequest request,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(
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
        UpdateUserIdentityApiRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
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
        ResetUserPasswordApiRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new ResetUserPasswordCommand(ActorUserId, id, request.Password),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/active")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> SetActive(
        Guid id,
        SetUserActiveApiRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new SetUserActiveCommand(ActorUserId, id, request.IsActive),
            cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:guid}/technician-profile")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> UpdateTechnicianProfile(
        Guid id,
        UpdateTechnicianProfileApiRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
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
        ChangeSupervisorCategoryApiRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new ChangeSupervisorCategoryCommand(
                ActorUserId,
                id,
                request.SupportCategoryId),
            cancellationToken);

        return NoContent();
    }
}

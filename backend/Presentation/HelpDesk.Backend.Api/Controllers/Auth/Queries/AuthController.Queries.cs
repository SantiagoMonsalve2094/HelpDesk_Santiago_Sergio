using HelpDesk.Backend.Application.DTOs.Auth;
using HelpDesk.Backend.Application.Features.Auth.Queries.GetCurrentUser;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Backend.Api.Controllers;

public sealed partial class AuthController
{
    [HttpGet("me")]
    [ProducesResponseType<AuthenticatedUserResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthenticatedUserResponse>> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetCurrentUserQuery(ActorUserId),
            cancellationToken);

        return Ok(response);
    }
}

using HelpDesk.Backend.Api.DTOs.Auth;
using HelpDesk.Backend.Application.Features.Auth.Commands.Login;
using HelpDesk.Backend.Application.DTOs.Auth;
using HelpDesk.Backend.Application.Features.Auth.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpDesk.Backend.Api.Controllers;

[Route("api/auth")]
public sealed class AuthController(ISender sender) : ApiControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginApiRequest request,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);
        return Ok(response);
    }

    [HttpGet("me")]
    [ProducesResponseType<AuthenticatedUserResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthenticatedUserResponse>> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(
            new GetCurrentUserQuery(ActorUserId),
            cancellationToken);
        return Ok(response);
    }
}

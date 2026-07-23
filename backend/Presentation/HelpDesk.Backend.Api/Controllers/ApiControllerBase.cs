using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Backend.Api.Controllers;

[ApiController]
[Authorize]
public abstract class ApiControllerBase : ControllerBase
{
    protected Guid ActorUserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("nameid") ??
                User.FindFirstValue("sub");
            return Guid.TryParse(value, out var userId)
                ? userId
                : throw new UnauthorizedAccessException(
                    "El token no contiene un identificador de usuario válido.");
        }
    }
}

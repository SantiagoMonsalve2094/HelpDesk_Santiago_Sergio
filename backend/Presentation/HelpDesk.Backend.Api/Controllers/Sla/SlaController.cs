using HelpDesk.Backend.Api.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Backend.Api.Controllers;

[Route("api/sla")]
[Authorize(Roles = RoleNames.SupervisorOrSuperAdmin)]
public sealed partial class SlaController : ApiControllerBase
{
    private readonly ISender _sender;

    public SlaController(ISender sender)
    {
        _sender = sender;
    }
}

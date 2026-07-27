using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Backend.Api.Controllers;

[Route("api/support-categories")]
public sealed partial class SupportCategoriesController : ApiControllerBase
{
    private readonly ISender _sender;

    public SupportCategoriesController(ISender sender)
    {
        _sender = sender;
    }

    private static TimeSpan Minutes(int value) =>
        TimeSpan.FromMinutes(value);
}

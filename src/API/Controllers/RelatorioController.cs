using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize]
public class RelatorioController : ControllerBase
{
    private readonly IMediator _mediator;

    public RelatorioController(IMediator mediator)
    {
        _mediator = mediator;
    }

}

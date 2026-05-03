using Application.Queries.ObterRelatorioServico;
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


    [HttpGet("relatorio/{hash}")]
    [AllowAnonymous]
    public async Task<IActionResult> ObterPorId(Guid hash, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObterRelatorioServicoQuery(hash), cancellationToken);

        if (result is null)
            return NotFound(new { message = "RElatório não encontrado." });

        return Ok(result);
    }

}

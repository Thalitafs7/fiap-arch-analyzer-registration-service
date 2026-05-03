using Application.Queries.ObterAnaliseAllServico;
using Application.Queries.ObterRelatorioAllServico;
using Application.Queries.ObterRelatorioServico;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api")]
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
            return NotFound(new { message = "Rlatório não encontrado." });

        return Ok(result);
    }

    [HttpGet("relatorio/all")]
    [AllowAnonymous]
    public async Task<IActionResult> ObterAll(CancellationToken cancellationToken)
    {

        var result = await _mediator.Send(new ObterRelatorioAllServicoQuery(), cancellationToken);

        if (result is null)
            return NotFound(new { message = "Analise não encontrada." });

        return Ok(result);
    }


}

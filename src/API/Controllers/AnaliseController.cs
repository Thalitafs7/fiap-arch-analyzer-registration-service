using Application.Commands.CriarAnalise;
using Application.Commands.DeletarAnalise;
using Application.Commands.UpdateAnalise;
using Application.Queries.ObterAnaliseAllServico;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace API.Controllers;

[ApiController]
[Route("api/analise")]
//[Authorize]
[ExcludeFromCodeCoverage]
public class AnaliseController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AnaliseController> _logger;

    public AnaliseController(
        IMediator mediator,
        ILogger<AnaliseController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromForm] CriarAnaliseRequest request)
    {
        var cancellationToken = new CancellationToken();

        var command = new CriarAnaliseCommand(Guid.NewGuid(), request.Descricao, request.Nome, request.Tipo, request.Files, request.FileType.ToString());
        var result = await _mediator.Send(command, cancellationToken);

        _logger.LogInformation("Analise criada com sucesso", result.Id, result.Nome);

        return Ok(result);

    }


    [HttpPut]
    public async Task<IActionResult> Update([FromForm] UpdateAnaliseRequest request)
    {
        var cancellationToken = new CancellationToken();

        var command = new UpdateAnaliseCommand(request.ClienteId, request.AnaliseId, request.Descricao, request.Nome);
        var result = await _mediator.Send(command, cancellationToken);

        _logger.LogInformation("Analise atualizada com sucesso", result.Id, result.Nome);
        return Ok(result);
    }


    [HttpDelete("analise/{hash}")]
    public async Task<IActionResult> Delete(Guid hash, CancellationToken cancellationToken)
    {
        //var cancellationToken = new CancellationToken();

        var command = new DeletarAnaliseCommand(hash);
        var result = await _mediator.Send(command, cancellationToken);

        _logger.LogInformation("Analise deletada com sucesso", result.Id, result.Nome);
        return Ok(result);
    }


    [HttpGet("all")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {

        var result = await _mediator.Send(new ObterAnaliseAllServicoQuery(), cancellationToken);

        if (result is null)
            return NotFound(new { message = "Analise não encontrada." });

        return Ok(result);
    }
}
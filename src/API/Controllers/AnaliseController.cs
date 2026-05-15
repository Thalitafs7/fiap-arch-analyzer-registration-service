using Application.Commands.CriarAnalise;
using Application.Commands.DeletarAnalise;
using Application.Commands.UpdateAnalise;
using Application.Common.Models;
using Application.Queries.ObterAnaliseAllServico;
using Application.Queries.ObterAnaliseServicoPorId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/analise")]
//[Authorize]
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
    public async Task<IActionResult> Criar([FromForm] CriarAnaliseRequest request, CancellationToken cancellationToken)
    {
        var files = new List<FileData>();
        using (var ms = new MemoryStream())
        {
            await request.File.CopyToAsync(ms, cancellationToken);
            files.Add(new FileData(ms.ToArray(), request.File.FileName, request.File.ContentType));
        }
        var command = new CriarAnaliseCommand(Guid.NewGuid(), request.Descricao, request.Nome, request.Tipo, files, null);
        var result = await _mediator.Send(command, cancellationToken);

        _logger.LogInformation("Analise criada com sucesso", result.Id, result.Nome);

        return Ok(result);

    }


    [HttpPut]
    public async Task<IActionResult> Update([FromForm] UpdateAnaliseRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateAnaliseCommand(request.ClienteId, request.AnaliseId, request.Descricao, request.Nome);
        var result = await _mediator.Send(command, cancellationToken);

        _logger.LogInformation("Analise atualizada com sucesso", result.Id, result.Nome);
        return Ok(result);
    }


    [HttpDelete("analise/{hash}")]
    public async Task<IActionResult> Delete(Guid hash, CancellationToken cancellationToken)
    {
        var command = new DeletarAnaliseCommand(hash);
        var result = await _mediator.Send(command, cancellationToken);

        _logger.LogInformation("Analise deletada com sucesso", result.Id, result.Nome);
        return Ok(result);
    }


    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObterAnaliseServicoPorIdQuery(id), cancellationToken);

        if (result is null)
            return NotFound(new { message = "Analise não encontrada." });

        return Ok(result);
    }


    [HttpGet("all")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {

        var result = await _mediator.Send(new ObterAnaliseAllServicoQuery(), cancellationToken);

        if (result is null)
            return NotFound(new { message = "Analise n�o encontrada." });

        return Ok(result);
    }
}
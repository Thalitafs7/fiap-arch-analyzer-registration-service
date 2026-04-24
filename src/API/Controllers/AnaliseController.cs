using Application.Commands.CriarAnalise;
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
}
using Application.Commands.CriarRelatorio;
using Application.Commands.AtualizarStatusAnalise;
using Application.Queries.ObterAnaliseServicoPorId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhookIAController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WebhookIAController> _logger;

    public WebhookIAController(
        IMediator mediator,
        ILogger<WebhookIAController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("report/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Criar([FromForm] CriarRelatorioRequest request, CancellationToken cancellationToken)
    {
        var command = new CriarRelatorioCommand(request.AnalysisId, request.Soat_analysis_id, request.Status, request.Report, request.ErrorMessage, request.CompletedAt);
        var result = await _mediator.Send(command, cancellationToken);
        _logger.LogInformation("Relatório atualizado com sucesso", result.Id, result.Nome);
        return Ok(result);
    }

    [HttpGet("analysis/{hash}")]
    [AllowAnonymous]
    public async Task<IActionResult> ObterPorId(Guid hash, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObterAnaliseServicoPorIdQuery(hash), cancellationToken);

        if (result is null)
            return NotFound(new { message = "Analise não encontrada." });

        return Ok(result);
    }

    [HttpPut("analysis/{hash}/status_processing")]
    public async Task<IActionResult> Update(Guid hash, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AtualizarStatusAnaliseCommand(hash), cancellationToken);
        _logger.LogInformation("Analise atualizada com sucesso", result.Id, result.Nome);
        return Ok(result);
    }
}

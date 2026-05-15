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

    /// <summary>
    /// Recebe o resultado da análise do processing-service via JSON (webhook callback).
    /// </summary>
    [HttpPost("report/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Criar([FromBody] CriarRelatorioRequest request, CancellationToken cancellationToken)
    {
        var command = new CriarRelatorioCommand(
            request.AnalysisId,
            request.Soat_analysis_id ?? Guid.Empty,
            request.Status,
            request.Report,
            request.ErrorMessage,
            request.CompletedAt);

        var result = await _mediator.Send(command, cancellationToken);
        _logger.LogInformation("Relatório salvo. Id={Id}", result.Id);
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

    /// <summary>
    /// Atualiza status via body — rota usada pelo processing-service (soat_client).
    /// PUT /api/webhooks/analyses/{hash}/status  body: {"status": "em_processamento"}
    /// </summary>
    [HttpPut("analyses/{hash}/status")]
    [AllowAnonymous]
    public async Task<IActionResult> AtualizarStatus(Guid hash, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AtualizarStatusAnaliseCommand(hash), cancellationToken);
        _logger.LogInformation("Status atualizado. Id={Id}", result?.Id);
        return Ok(result);
    }

    /// <summary>Rota legada — mantida para compatibilidade.</summary>
    [HttpPut("analysis/{hash}/status_processing")]
    [AllowAnonymous]
    public async Task<IActionResult> Update(Guid hash, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AtualizarStatusAnaliseCommand(hash), cancellationToken);
        return Ok(result);
    }
}

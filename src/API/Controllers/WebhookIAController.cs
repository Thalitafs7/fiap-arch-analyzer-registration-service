using Application.Commands.CriarRelatorio;
using Application.Common.Interfaces;
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
    private readonly IWebhookSignatureValidator _signatureValidator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhookIAController> _logger;

    public WebhookIAController(
        IMediator mediator,
        IWebhookSignatureValidator signatureValidator,

        IConfiguration configuration,
        ILogger<WebhookIAController> logger)
    {
        _mediator = mediator;
        _signatureValidator = signatureValidator;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("relatorios")]
    [AllowAnonymous]
    public async Task<IActionResult> Criar([FromForm] CriarRelatorioRequest request)
    {
        var cancellationToken = new CancellationToken();

        var command = new CriarRelatorioCommand(request.AnaliseId, request.DiagramaId, request.Nome, request.URLS3Relatorio);
        var result = await _mediator.Send(command, cancellationToken);
        _logger.LogInformation("Relatório atualizado com sucesso", result.Id, result.Nome);
        return Ok(result);

    }


    [HttpGet("analise/{hash}")]
    [AllowAnonymous]
    public async Task<IActionResult> ObterPorId(Guid hash, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObterAnaliseServicoPorIdQuery(hash), cancellationToken);

        if (result is null)
            return NotFound(new { message = "Analise não encontrada." });

        return Ok(result);
    }



}



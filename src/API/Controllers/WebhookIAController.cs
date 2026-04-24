using Application.Common.Interfaces;
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

    [HttpPost("mercadopago")]
    [AllowAnonymous]
    public async Task<IActionResult> MercadoPagoWebhook(
        [FromQuery(Name = "data.id")] string? dataId,
        [FromHeader(Name = "x-signature")] string? xSignature,
        [FromHeader(Name = "x-request-id")] string? xRequestId,
        [FromBody] MercadoPagoWebhookPayload payload,
        CancellationToken cancellationToken)
    {
        try
        {
            var webhookSecret = _configuration["MercadoPago:WebhookSecret"];
            var skipValidation = _configuration.GetValue<bool>("MercadoPago:SkipWebhookValidation");

            if (!skipValidation && !string.IsNullOrEmpty(webhookSecret))
            {
                if (string.IsNullOrEmpty(xSignature) || string.IsNullOrEmpty(xRequestId) || string.IsNullOrEmpty(dataId))
                {
                    _logger.LogWarning("Webhook rejeitado: headers obrigatórios ausentes");
                    return BadRequest(new { message = "Headers obrigatórios ausentes (x-signature, x-request-id, data.id)" });
                }

                if (!_signatureValidator.ValidateSignature(xSignature, xRequestId, dataId, webhookSecret))
                {
                    _logger.LogWarning("Webhook rejeitado: assinatura inválida - DataId: {DataId}", dataId);
                    return Unauthorized(new { message = "Assinatura inválida" });
                }
            }

            if (payload.Type != "payment" && payload.Action != "payment.updated" && payload.Action != "payment.created")
            {
                _logger.LogInformation("Webhook ignorado - Type: {Type}, Action: {Action}", payload.Type, payload.Action);
                return Ok(new { message = "Event type não processado" });
            }

            var paymentId = dataId ?? payload.Data?.Id;
            if (string.IsNullOrEmpty(paymentId))
            {
                _logger.LogWarning("Webhook rejeitado: PaymentId não encontrado");
                return BadRequest(new { message = "PaymentId é obrigatório" });
            }


            return Ok(new { message = $"Status  registrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook Mercado Pago");
            return StatusCode(500, new { message = "Erro interno ao processar webhook" });
        }
    }

}

public record MercadoPagoWebhookPayload(
    long Id,
    bool LiveMode,
    string Type,
    DateTime DateCreated,
    long UserId,
    string ApiVersion,
    string Action,
    MercadoPagoWebhookData? Data,
    string? ExternalReference
);

public record MercadoPagoWebhookData(string Id);

using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Security;

public class WebhookSignatureValidator : IWebhookSignatureValidator
{
    private readonly ILogger<WebhookSignatureValidator> _logger;
    private const int TimestampToleranceMinutes = 5;

    public WebhookSignatureValidator(ILogger<WebhookSignatureValidator> logger)
    {
        _logger = logger;
    }

    public bool ValidateSignature(string xSignature, string xRequestId, string dataId, string secret)
    {
        try
        {
            if (string.IsNullOrEmpty(xSignature) || string.IsNullOrEmpty(xRequestId) ||
                string.IsNullOrEmpty(dataId) || string.IsNullOrEmpty(secret))
            {
                _logger.LogWarning("Validação de assinatura falhou: parâmetros obrigatórios ausentes");
                return false;
            }

            var (ts, hash) = ExtractTimestampAndHash(xSignature);

            if (string.IsNullOrEmpty(ts) || string.IsNullOrEmpty(hash))
            {
                _logger.LogWarning("x-signature inválido: ts ou hash ausente. Signature: {Signature}", xSignature);
                return false;
            }

            if (!ValidateTimestamp(ts))
            {
                return false;
            }

            var manifest = BuildManifest(dataId, xRequestId, ts);
            var computedHash = ComputeHmacSha256(manifest, secret);
            var isValid = string.Equals(computedHash, hash, StringComparison.OrdinalIgnoreCase);

            if (!isValid)
            {
                _logger.LogWarning("Assinatura inválida - Expected: {Expected}, Received: {Received}",
                    computedHash, hash);
            }
            else
            {
                _logger.LogDebug("Assinatura validada com sucesso - DataId: {DataId}", dataId);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar assinatura webhook");
            return false;
        }
    }

    private (string? ts, string? hash) ExtractTimestampAndHash(string xSignature)
    {
        string? ts = null;
        string? hash = null;

        var parts = xSignature.Split(',');
        foreach (var part in parts)
        {
            var keyValue = part.Split('=', 2);
            if (keyValue.Length == 2)
            {
                var key = keyValue[0].Trim();
                var value = keyValue[1].Trim();

                if (key == "ts")
                    ts = value;
                else if (key == "v1")
                    hash = value;
            }
        }

        return (ts, hash);
    }

    private bool ValidateTimestamp(string ts)
    {
        if (!long.TryParse(ts, out var timestamp))
        {
            _logger.LogWarning("Timestamp inválido: {Timestamp}", ts);
            return false;
        }

        var notificationTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        var now = DateTimeOffset.UtcNow;
        var diff = Math.Abs((now - notificationTime).TotalMinutes);

        if (diff > TimestampToleranceMinutes)
        {
            _logger.LogWarning("Timestamp expirado - Diff: {Diff} minutos, Tolerance: {Tolerance} minutos",
                diff, TimestampToleranceMinutes);
            return false;
        }

        return true;
    }

    private static string BuildManifest(string dataId, string xRequestId, string ts)
    {
        return $"id:{dataId};request-id:{xRequestId};ts:{ts};";
    }

    private static string ComputeHmacSha256(string message, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}

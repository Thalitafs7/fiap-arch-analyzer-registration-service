using Application.Common.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace API.HealthChecks;


public class CadastrosServiceHealthCheck : IHealthCheck
{
    private readonly ICadastrosService _cadastrosService;
    private readonly ILogger<CadastrosServiceHealthCheck> _logger;

    private const int LatencyThresholdMs = 5000; // 5 segundos

    public CadastrosServiceHealthCheck(
        ICadastrosService cadastrosService,
        ILogger<CadastrosServiceHealthCheck> logger)
    {
        _cadastrosService = cadastrosService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {


            await _cadastrosService.ValidarClienteVeiculoAsync(
                Guid.Empty,
                Guid.Empty,
                cancellationToken);

            stopwatch.Stop();
            var latencyMs = stopwatch.ElapsedMilliseconds;

            var data = new Dictionary<string, object>
            {
                { "latency_ms", latencyMs },
                { "endpoint", "ms-cadastros" }
            };

            if (latencyMs > LatencyThresholdMs)
            {
                _logger.LogWarning(
                    "ms-cadastros respondendo lentamente: {Latency}ms",
                    latencyMs);

                return HealthCheckResult.Degraded(
                    $"ms-cadastros respondendo lentamente ({latencyMs}ms)",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                $"ms-cadastros saudável ({latencyMs}ms)",
                data: data);
        }
        catch (Polly.CircuitBreaker.BrokenCircuitException ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "ms-cadastros indisponível - Circuit Breaker aberto");

            return HealthCheckResult.Unhealthy(
                "ms-cadastros indisponível (Circuit Breaker aberto)",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "error", "circuit_breaker_open" },
                    { "latency_ms", stopwatch.ElapsedMilliseconds }
                });
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            stopwatch.Stop();

            _logger.LogError(ex, "ms-cadastros timeout");

            return HealthCheckResult.Unhealthy(
                "ms-cadastros timeout",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "error", "timeout" },
                    { "latency_ms", stopwatch.ElapsedMilliseconds }
                });
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex, "ms-cadastros erro de conexão");

            return HealthCheckResult.Unhealthy(
                $"ms-cadastros erro de conexão: {ex.Message}",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "error", "connection_error" },
                    { "latency_ms", stopwatch.ElapsedMilliseconds }
                });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex, "ms-cadastros erro inesperado");

            return HealthCheckResult.Unhealthy(
                $"ms-cadastros erro: {ex.Message}",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "error", "unexpected" },
                    { "latency_ms", stopwatch.ElapsedMilliseconds }
                });
        }
    }
}

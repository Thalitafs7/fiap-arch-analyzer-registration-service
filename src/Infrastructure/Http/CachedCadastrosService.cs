using Application.Common.DTOs;
using Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Http;


public class CachedCadastrosService : ICadastrosService
{
    private readonly ICadastrosService _inner;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedCadastrosService> _logger;

    private static readonly TimeSpan ValidacaoCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ServicoCacheDuration = TimeSpan.FromMinutes(10);

    public CachedCadastrosService(
        ICadastrosService inner,
        IMemoryCache cache,
        ILogger<CachedCadastrosService> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> ValidarClienteVeiculoAsync(
        Guid clienteId,
        Guid veiculoId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"validacao:cliente:{clienteId}:veiculo:{veiculoId}";

        if (_cache.TryGetValue(cacheKey, out bool cachedResult))
        {
            _logger.LogDebug(
                "Cache HIT para validação cliente/veículo: {ClienteId}/{VeiculoId}",
                clienteId, veiculoId);
            return cachedResult;
        }

        _logger.LogDebug(
            "Cache MISS para validação cliente/veículo: {ClienteId}/{VeiculoId}",
            clienteId, veiculoId);

        try
        {
            var resultado = await _inner.ValidarClienteVeiculoAsync(clienteId, veiculoId, cancellationToken);

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ValidacaoCacheDuration,
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            _cache.Set(cacheKey, resultado, cacheOptions);

            _logger.LogDebug(
                "Validação cliente/veículo cacheada: {ClienteId}/{VeiculoId} = {Resultado}",
                clienteId, veiculoId, resultado);

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Erro ao validar cliente/veículo {ClienteId}/{VeiculoId}. Verificando cache para fallback.",
                clienteId, veiculoId);

            if (_cache.TryGetValue(cacheKey, out bool staleResult))
            {
                _logger.LogWarning(
                    "Usando cache stale para validação {ClienteId}/{VeiculoId} devido a erro",
                    clienteId, veiculoId);
                return staleResult;
            }

            throw;
        }
    }


    public async Task<ClienteDto?> ObterClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"cliente:{clienteId}";

        if (_cache.TryGetValue(cacheKey, out ClienteDto? cachedCliente))
        {
            _logger.LogDebug("Cache HIT para cliente: {ClienteId}", clienteId);
            return cachedCliente;
        }

        _logger.LogDebug("Cache MISS para cliente: {ClienteId}", clienteId);

        try
        {
            var cliente = await _inner.ObterClienteAsync(clienteId, cancellationToken);

            if (cliente != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                };

                _cache.Set(cacheKey, cliente, cacheOptions);
                _logger.LogDebug("Cliente cacheado: {ClienteId}", clienteId);
            }

            return cliente;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao obter cliente {ClienteId}. Verificando cache para fallback.", clienteId);

            if (_cache.TryGetValue(cacheKey, out ClienteDto? staleCliente))
            {
                _logger.LogWarning("Usando cache stale para cliente {ClienteId} devido a erro", clienteId);
                return staleCliente;
            }

            throw;
        }
    }
}

using Application.Common.DTOs;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Infrastructure.ExternalServices;

public class CadastrosApiClient : ICadastrosService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CadastrosApiClient> _logger;

    public CadastrosApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<CadastrosApiClient> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(configuration["Services:MsCadastros:BaseUrl"] ?? "http://localhost:5001");
        _logger = logger;
    }

    public async Task<bool> ValidarClienteVeiculoAsync(Guid clienteId, Guid veiculoId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/clientes/{clienteId}/veiculos/{veiculoId}/validar",
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar cliente/veículo: {ClienteId}/{VeiculoId}", clienteId, veiculoId);
            return false;
        }
    }

    public async Task<ClienteDto?> ObterClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/clientes/{clienteId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ClienteDto>(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter cliente: {ClienteId}", clienteId);
            return null;
        }
    }
}

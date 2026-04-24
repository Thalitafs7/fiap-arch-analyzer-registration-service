using System.Net.Http.Json;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Clients;

public class CadastrosApiClient
{
    private readonly HttpClient _httpClient;

    public CadastrosApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ClienteDto> CriarClienteAsync(CriarClienteRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/clientes", request, JsonSerializerHelper.CamelCaseOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ClienteDto>(JsonSerializerHelper.CamelCaseOptions))!;
    }

    public async Task<ClienteDto?> ObterClientePorIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<ClienteDto>($"/api/clientes/{id}", JsonSerializerHelper.CamelCaseOptions);
    }

    public async Task<VeiculoDto> AdicionarVeiculoAsync(Guid clienteId, CriarVeiculoRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/clientes/{clienteId}/veiculos", request, JsonSerializerHelper.CamelCaseOptions);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Erro ao adicionar veículo: {response.StatusCode} - {errorContent}");
        }

        return (await response.Content.ReadFromJsonAsync<VeiculoDto>(JsonSerializerHelper.CamelCaseOptions))!;
    }

    public async Task<ServicoDto> CriarServicoAsync(CriarServicoRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/servicos", request, JsonSerializerHelper.CamelCaseOptions);

        // Se já existe, tenta criar com nome único
        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var uniqueRequest = request with { Nome = $"{request.Nome}_{Guid.NewGuid():N}" };
            response = await _httpClient.PostAsJsonAsync("/api/servicos", uniqueRequest, JsonSerializerHelper.CamelCaseOptions);
        }

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ServicoDto>(JsonSerializerHelper.CamelCaseOptions))!;
    }

    public async Task<ServicoDto?> ObterServicoPorIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<ServicoDto>($"/api/servicos/{id}", JsonSerializerHelper.CamelCaseOptions);
    }

    public async Task<bool> ValidarClienteVeiculoAsync(Guid clienteId, Guid veiculoId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/clientes/{clienteId}/veiculos/{veiculoId}/validar");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ServicoDto?> ObterServicoAsync(Guid servicoId)
    {
        return await ObterServicoPorIdAsync(servicoId);
    }

    public async Task<ClienteDto?> ObterClienteAsync(Guid clienteId)
    {
        return await ObterClientePorIdAsync(clienteId);
    }
}

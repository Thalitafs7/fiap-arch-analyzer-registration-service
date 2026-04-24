using System.Net.Http.Json;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Clients;

public class EstoqueApiClient
{
    private readonly HttpClient _httpClient;

    public EstoqueApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<EstoqueDto> CriarEstoqueAsync(CriarEstoqueRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/estoques", request, JsonSerializerHelper.CamelCaseOptions);

        // Se já existe, tenta buscar pelo nome ou retorna o erro
        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            // Criar um novo request com nome único
            var uniqueRequest = request with { Insumo = $"{request.Insumo}_{Guid.NewGuid():N}" };
            response = await _httpClient.PostAsJsonAsync("/api/estoques", uniqueRequest, JsonSerializerHelper.CamelCaseOptions);
        }

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<EstoqueDto>(JsonSerializerHelper.CamelCaseOptions))!;
    }

    public async Task<EstoqueDto?> ObterPorIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<EstoqueDto>($"/api/estoques/{id}", JsonSerializerHelper.CamelCaseOptions);
    }

    public async Task<IEnumerable<ReservaDto>> ObterReservasPorOrdemServicoAsync(Guid ordemServicoId)
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<ReservaDto>>(
            $"/api/reservas/ordem-servico/{ordemServicoId}",
            JsonSerializerHelper.CamelCaseOptions) ?? Enumerable.Empty<ReservaDto>();
    }

    public async Task<AlertaEstoqueDto?> ObterAlertaPorEstoqueIdAsync(Guid estoqueId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/alertas/estoque/{estoqueId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AlertaEstoqueDto>(JsonSerializerHelper.CamelCaseOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<AlertaEstoqueDto>> ObterAlertasNaoNotificadosAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<AlertaEstoqueDto>>(
                "/api/alertas",
                JsonSerializerHelper.CamelCaseOptions);
            return result ?? new List<AlertaEstoqueDto>();
        }
        catch
        {
            return new List<AlertaEstoqueDto>();
        }
    }
}

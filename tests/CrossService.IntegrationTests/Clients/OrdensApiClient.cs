using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Clients;

public class OrdensApiClient
{
    private readonly HttpClient _httpClient;

    public OrdensApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OrdemServicoDto> CriarAsync(CriarOrdemServicoRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/ordens", request, JsonSerializerHelper.CamelCaseOptions);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Erro ao criar OS: {response.StatusCode} - {errorContent}");
        }

        return (await response.Content.ReadFromJsonAsync<OrdemServicoDto>(JsonSerializerHelper.CamelCaseOptions))!;
    }

    public async Task<OrdemServicoDto?> TentarCriarAsync(CriarOrdemServicoRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/ordens", request, JsonSerializerHelper.CamelCaseOptions);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<OrdemServicoDto>(JsonSerializerHelper.CamelCaseOptions);
    }

    public async Task<OrdemServicoDto?> ObterPorIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<OrdemServicoDto>($"/api/ordens/{id}", JsonSerializerHelper.CamelCaseOptions);
    }

    public async Task<IEnumerable<OrdemServicoDto>> ObterPorClienteAsync(Guid clienteId)
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<OrdemServicoDto>>(
            $"/api/ordens/cliente/{clienteId}",
            JsonSerializerHelper.CamelCaseOptions) ?? Enumerable.Empty<OrdemServicoDto>();
    }

    public async Task<IEnumerable<OrdemServicoDto>> ObterPorStatusAsync(StatusOrdemServico status)
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<OrdemServicoDto>>(
            $"/api/ordens/status/{(int)status}",
            JsonSerializerHelper.CamelCaseOptions) ?? Enumerable.Empty<OrdemServicoDto>();
    }

    public async Task<OrdemServicoDto> GerarOrcamentoAsync(Guid ordemServicoId, GerarOrcamentoRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"/api/ordens/{ordemServicoId}/orcamento",
            request,
            JsonSerializerHelper.CamelCaseOptions);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Erro ao gerar orçamento: {response.StatusCode} - {errorContent}");
        }

        return (await response.Content.ReadFromJsonAsync<OrdemServicoDto>(JsonSerializerHelper.CamelCaseOptions))!;
    }

    public async Task<OrdemServicoDto> AprovarOrcamentoAsync(Guid ordemServicoId, bool aprovar)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"/api/ordens/{ordemServicoId}/orcamento/aprovar",
            new { Aprovado = aprovar },
            JsonSerializerHelper.CamelCaseOptions);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Erro ao aprovar orçamento: {response.StatusCode} - {errorContent}");
        }

        return (await response.Content.ReadFromJsonAsync<OrdemServicoDto>(JsonSerializerHelper.CamelCaseOptions))!;
    }

    public async Task<PagamentoDto> IniciarPagamentoPIXAsync(Guid ordemServicoId, string emailPagador)
    {
        var request = new { MetodoPagamento = MetodoPagamento.Pix, EmailPagador = emailPagador };
        var response = await _httpClient.PostAsJsonAsync(
            $"/api/ordens/{ordemServicoId}/pagamento",
            request,
            JsonSerializerHelper.RequestOptions);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Erro ao iniciar pagamento: {response.StatusCode} - {errorContent}");
        }

        return (await response.Content.ReadFromJsonAsync<PagamentoDto>(JsonSerializerHelper.CamelCaseOptions))!;
    }

    public async Task<PagamentoDto> IniciarPagamentoCartaoAsync(Guid ordemServicoId, string emailPagador = "cliente@teste.com")
    {
        var request = new { MetodoPagamento = MetodoPagamento.CartaoCredito, EmailPagador = emailPagador };
        var response = await _httpClient.PostAsJsonAsync(
            $"/api/ordens/{ordemServicoId}/pagamento",
            request,
            JsonSerializerHelper.RequestOptions);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Erro ao iniciar pagamento: {response.StatusCode} - {errorContent}");
        }

        return (await response.Content.ReadFromJsonAsync<PagamentoDto>(JsonSerializerHelper.CamelCaseOptions))!;
    }

    public async Task<OrdemServicoDto> GerarOrcamentoAsync(Guid ordemServicoId, decimal valorServico = 150.00m, decimal valorInsumos = 0.00m, int diasValidade = 7)
    {
        var request = new GerarOrcamentoRequest(ordemServicoId, valorServico, valorInsumos, diasValidade);
        return await GerarOrcamentoAsync(ordemServicoId, request);
    }

    public async Task<PagamentoDto?> IniciarPagamentoAsync(Guid ordemServicoId, string metodoPagamento = "PIX", string emailPagador = "cliente@teste.com")
    {
        var metodo = metodoPagamento == "PIX" ? MetodoPagamento.Pix : MetodoPagamento.CartaoCredito;
        var request = new { MetodoPagamento = metodo, EmailPagador = emailPagador };
        var response = await _httpClient.PostAsJsonAsync(
            $"/api/ordens/{ordemServicoId}/pagamento",
            request,
            JsonSerializerHelper.RequestOptions);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PagamentoDto>(JsonSerializerHelper.CamelCaseOptions);
    }

    public async Task<OrdemServicoDto?> SimularWebhookPagamentoAsync(Guid ordemServicoId, string status, string? motivo = null)
    {
        var request = new { OrdemServicoId = ordemServicoId, Status = status, PaymentId = Guid.NewGuid().ToString() };
        var response = await _httpClient.PostAsJsonAsync(
            "/api/webhooks/mercadopago/legacy",
            request,
            JsonSerializerHelper.CamelCaseOptions);

        if (!response.IsSuccessStatusCode) return null;

        // O endpoint legacy retorna o resultado do comando, buscar a OS atualizada
        await Task.Delay(500); // Aguardar processamento
        return await ObterPorIdAsync(ordemServicoId);
    }

    public async Task<int> DispararJobExpirarPagamentosAsync()
    {
        var response = await _httpClient.PostAsync("/api/jobs/expirar-pagamentos", null);
        if (!response.IsSuccessStatusCode) return 0;
        var result = await response.Content.ReadFromJsonAsync<JobResultDto>(JsonSerializerHelper.CamelCaseOptions);
        return result?.Expirados ?? 0;
    }

    // ============================================
    // CHECKLIST DE INSPEÇÃO
    // ============================================

    public async Task<ChecklistResponseDto?> CriarChecklistAsync(CriarChecklistDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/checklists", request, JsonSerializerHelper.RequestOptions);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Erro ao criar checklist: {response.StatusCode} - {errorContent}");
        }
        return await response.Content.ReadFromJsonAsync<ChecklistResponseDto>(JsonSerializerHelper.CamelCaseOptions);
    }

    public async Task<ChecklistResponseDto?> ObterChecklistPorIdAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ChecklistResponseDto>(
                $"/api/checklists/{id}", JsonSerializerHelper.CamelCaseOptions);
        }
        catch { return null; }
    }

    public async Task<IEnumerable<ChecklistResponseDto>?> ObterChecklistsPorOrdemServicoAsync(Guid ordemServicoId)
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<ChecklistResponseDto>>(
            $"/api/checklists/ordem/{ordemServicoId}", JsonSerializerHelper.CamelCaseOptions);
    }

    public async Task<IEnumerable<ChecklistResponseDto>?> ObterChecklistsPorVeiculoAsync(Guid veiculoId, int limite = 10)
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<ChecklistResponseDto>>(
            $"/api/checklists/veiculo/{veiculoId}?limite={limite}", JsonSerializerHelper.CamelCaseOptions);
    }

    public async Task<IEnumerable<ProblemaFrequenteDto>?> ObterProblemasFrequentesAsync(string marca, string modelo, int limite = 10)
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<ProblemaFrequenteDto>>(
            $"/api/checklists/problemas-frequentes?marca={marca}&modelo={modelo}&limite={limite}",
            JsonSerializerHelper.CamelCaseOptions);
    }

    public async Task<EstatisticasInspecaoDto?> ObterEstatisticasChecklistAsync(DateTime? inicio = null, DateTime? fim = null)
    {
        var query = new List<string>();
        if (inicio.HasValue) query.Add($"inicio={inicio.Value:yyyy-MM-ddTHH:mm:ssZ}");
        if (fim.HasValue) query.Add($"fim={fim.Value:yyyy-MM-ddTHH:mm:ssZ}");
        var queryString = query.Any() ? "?" + string.Join("&", query) : "";

        return await _httpClient.GetFromJsonAsync<EstatisticasInspecaoDto>(
            $"/api/checklists/estatisticas{queryString}", JsonSerializerHelper.CamelCaseOptions);
    }

    // ============================================
    // OUTBOX PATTERN
    // ============================================

    public async Task<OutboxMessageDto?> ObterMensagemOutboxPorOrdemServicoAsync(Guid ordemServicoId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<OutboxMessageDto>(
                $"/api/debug/outbox/ordem/{ordemServicoId}", JsonSerializerHelper.CamelCaseOptions);
        }
        catch { return null; }
    }

    public async Task<OrdemServicoDto?> CriarComCorrelationIdAsync(CriarOrdemServicoRequest request, string correlationId)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/ordens")
        {
            Content = JsonContent.Create(request, options: JsonSerializerHelper.CamelCaseOptions)
        };
        requestMessage.Headers.Add("X-Correlation-ID", correlationId);

        var response = await _httpClient.SendAsync(requestMessage);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<OrdemServicoDto>(JsonSerializerHelper.CamelCaseOptions);
    }
}

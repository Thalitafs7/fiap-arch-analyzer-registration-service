using System.Net.Http.Json;
using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Http;

public class ProcessingServiceClient : IProcessingServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProcessingServiceClient> _logger;

    public ProcessingServiceClient(HttpClient httpClient, ILogger<ProcessingServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProcessingAnalysisStatus?> GetAnalysisStatusAsync(Guid processingAnalysisId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/analyses/{processingAnalysisId}/status", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Análise {Id} não encontrada no processing-service.", processingAnalysisId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<ProcessingStatusResponse>(cancellationToken: cancellationToken);
            if (dto is null)
                return null;

            return new ProcessingAnalysisStatus(
                dto.analysis_id ?? processingAnalysisId.ToString(),
                dto.status ?? string.Empty,
                dto.error_message,
                dto.external_analysis_id
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao consultar status no processing-service para análise {Id}.", processingAnalysisId);
            return null;
        }
    }

    private record ProcessingStatusResponse(
        string? analysis_id,
        string? status,
        string? file_name,
        string? external_analysis_id,
        string? error_message,
        string? created_at
    );
}

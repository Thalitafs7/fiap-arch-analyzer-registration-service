namespace Application.Common.Interfaces;

public interface IProcessingServiceClient
{
    /// <summary>
    /// Consulta o status de uma análise no processing-service pelo ID interno do processing.
    /// Retorna null se a análise não for encontrada ou se o serviço estiver indisponível.
    /// </summary>
    Task<ProcessingAnalysisStatus?> GetAnalysisStatusAsync(Guid processingAnalysisId, CancellationToken cancellationToken = default);
}

public record ProcessingAnalysisStatus(
    string AnalysisId,
    string Status,
    string? ErrorMessage,
    string? ExternalAnalysisId
);

namespace Application.Common.Interfaces;

public interface IRabbitMQDiagramPublisher
{
    Task PublishAsync(DiagramMessage message);
}

public record DiagramMessage(
    string JobId,
    string FileName,
    string FileB64,
    string ContentType,
    string SoatAnalysisId,
    string CallbackUrl
);

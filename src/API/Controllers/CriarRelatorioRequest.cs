using Application.Commands.CriarRelatorio;
using System.Text.Json.Serialization;

namespace API.Controllers;

public record CriarRelatorioRequest
{
    [JsonPropertyName("analysis_id")]
    public Guid AnalysisId { get; init; }

    [JsonPropertyName("soat_analysis_id")]
    public Guid? Soat_analysis_id { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("report")]
    public ReportDetail? Report { get; init; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; init; }

    [JsonPropertyName("error_step")]
    public string? ErrorStep { get; init; }

    [JsonPropertyName("error_type")]
    public string? ErrorType { get; init; }

    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; init; }
}

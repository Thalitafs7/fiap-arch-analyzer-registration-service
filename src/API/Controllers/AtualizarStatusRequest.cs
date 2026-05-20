using System.Text.Json.Serialization;

namespace API.Controllers;

public record AtualizarStatusRequest
{
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("soat_analysis_id")]
    public Guid? SoatAnalysisId { get; init; }
}

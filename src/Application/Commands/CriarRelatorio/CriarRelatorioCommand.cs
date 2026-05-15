using Application.DTOs;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Commands.CriarRelatorio;

public record CriarRelatorioCommand(
    Guid AnalysisId,
    Guid soat_analysis_id,
    string? Status,
    ReportDetail? Report,
    string? ErrorMessage,
    DateTimeOffset? CompletedAt
) : IRequest<RelatorioDto>;

public record ReportDetail
{
    [JsonPropertyName("components_identified")]
    public List<string>? ComponentsIdentified { get; set; }

    [JsonPropertyName("architectural_risks")]
    public List<string>? ArchitecturalRisks { get; set; }

    [JsonPropertyName("recommendations")]
    public List<string>? Recommendations { get; set; }

    [JsonPropertyName("executive_summary")]
    public string? ExecutiveSummary { get; set; }

    [JsonPropertyName("rag_used")]
    public bool RagUsed { get; set; }
}

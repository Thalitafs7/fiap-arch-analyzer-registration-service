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
    string? ErrorStep,
    string? ErrorType,
    DateTimeOffset? CompletedAt
) : IRequest<RelatorioDto>;

public record ReportDetail
{
    [JsonPropertyName("components_identified")]
    public List<string>? ComponentsIdentified { get; set; }

    [JsonPropertyName("architectural_risks")]
    public List<ArchitecturalRiskDetail>? ArchitecturalRisks { get; set; }

    [JsonPropertyName("recommendations")]
    public List<string>? Recommendations { get; set; }

    [JsonPropertyName("executive_summary")]
    public string? ExecutiveSummary { get; set; }

    [JsonPropertyName("rag_used")]
    public bool RagUsed { get; set; }
}

public record ArchitecturalRiskDetail
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("severity")]
    public string? Severity { get; set; }

    [JsonPropertyName("affected_components")]
    public List<string>? AffectedComponents { get; set; }

    [JsonPropertyName("mitigation")]
    public string? Mitigation { get; set; }

    public override string ToString() =>
        $"[{Severity}] {Type}: {Description} — Mitigação: {Mitigation}";
}

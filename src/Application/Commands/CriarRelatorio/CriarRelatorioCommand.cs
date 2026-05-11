using Application.DTOs;
using MediatR;

namespace Application.Commands.CriarRelatorio;

public record CriarRelatorioCommand(         
    Guid AnalysisId,
    Guid soat_analysis_id,
    string? Status,
    ReportDetail Report,
    string? ErrorMessage,
    DateTimeOffset? CompletedAt
) : IRequest<RelatorioDto>;

public record ReportDetail
{
    public List<string>? ComponentsIdentified { get; set; }
    public List<string>? ArchitecturalRisks { get; set; }
    public List<string>? Recommendations { get; set; }
    public string? ExecutiveSummary { get; set; }
    public bool RagUsed { get; set; }
}

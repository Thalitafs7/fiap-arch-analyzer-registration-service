using Application.Commands.CriarRelatorio;
using System.Text.Json.Serialization;

namespace API.Controllers;


public record CriarRelatorioRequest
{
    public required Guid AnalysisId { get; init; }
    public required Guid Soat_analysis_id { get; init; }
    public string? Status { get; init; }
    public ReportDetail? Report { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}

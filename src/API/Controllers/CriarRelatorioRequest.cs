using Application.Commands.CriarRelatorio;
using System.Text.Json.Serialization;

namespace API.Controllers;


public record CriarRelatorioRequest(
    Guid AnalysisId,
    Guid Soat_analysis_id,
    string? Status,
    ReportDetail Report,
    string? ErrorMessage,
    DateTimeOffset? CompletedAt
);


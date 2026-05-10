using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Logging;

[ExcludeFromCodeCoverage]
public class LogEntryDto
{
    public string? Nivel { get; set; }
    public string? Classe { get; set; }
    public string? Metodo { get; set; }
    public string Etapa { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public string? TraceId { get; set; }
    public string? SpanId { get; set; }
    public object? Dados { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Usuario { get; set; }
}

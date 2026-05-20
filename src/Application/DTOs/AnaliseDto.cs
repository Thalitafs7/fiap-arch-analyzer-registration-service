namespace Application.DTOs;

public record AnaliseDto(
    Guid ClienteId,
    Guid Id,
    string? Nome,
    string? Status,
    Guid? SoatAnalysisId,
    string? Descricao,
    List<DiagramaDto> Diagramas,
    DateTime DataCadastro
);


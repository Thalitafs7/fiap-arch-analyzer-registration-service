namespace Application.DTOs;

public record AnaliseDto(
    Guid ClienteId,
    Guid Id,
    string? Nome,
    string? Status,
    List<DiagramaDto> Diagramas,
    DateTime DataCadastro
);


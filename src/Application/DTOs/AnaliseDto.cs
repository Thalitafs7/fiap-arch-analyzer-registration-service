namespace Application.DTOs;

public record AnaliseDto(
    Guid Id,
    string? Nome,
    string? Status,
    List<DiagramaDto> Diagramas,
    DateTime DataCadastro
);


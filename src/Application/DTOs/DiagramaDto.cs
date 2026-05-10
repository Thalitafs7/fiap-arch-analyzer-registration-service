namespace Application.DTOs;

public record DiagramaDto(
     Guid Id,
     string? Nome,
     string? TipoDiagrama,
     string? URLS3Diagrama,
     RelatorioDto? Relatorio
 );

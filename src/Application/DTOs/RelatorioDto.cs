namespace Application.DTOs;

public record RelatorioDto(
    Guid Id,
    string? Nome,
    Guid? Soat_Analysis_Id,
    Guid? IdDiagrama,
    List<string>? Componentes_Identificado,
    List<string>? Risco_Arquitetura,
    List<string>? Recomendacao,
    string? Message_Error
);

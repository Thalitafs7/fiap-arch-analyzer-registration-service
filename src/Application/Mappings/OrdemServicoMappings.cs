using Application.DTOs;
using Domain.Entities;

namespace Application.Mappings;

public static class OrdemServicoMappings
{
    public static AnaliseDto ToDto(this Analise an)
    {
        return new AnaliseDto(
            an.Id,
            an.Status,
            an.Nome,
            an.Diagramas.Select(d => d.ToDto()).ToList(),
            an.DataCadastro
        );
    }

    public static DiagramaDto ToDto(this Diagrama diag)
    {
        return new DiagramaDto(
            diag.Id,
            diag.Nome,
            diag.TipoDiagrama,
            diag.URLS3Diagrama,
            diag.Relatorio.ToDto()
        );
    }

    public static RelatorioDto ToDto(this Relatorio rel)
    {
        return new RelatorioDto(
            rel.Id,
            rel.Nome,
            rel.URLS3Relatorio
        );
    }
}

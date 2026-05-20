using Application.DTOs;
using Domain.Entities;

namespace Application.Mappings;

public static class AnaliseServicoMappings
{
    public static AnaliseDto ToDto(this Analise an)
    {
        return new AnaliseDto(
            an.ClienteId,
            an.Id,
            an.Nome,
            an.Status.ToString(),
            an.SoatAnalysisId,
            an.Descricao,
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
            diag.Relatorio?.ToDto()
        );
    }

    public static RelatorioDto ToDto(this Relatorio rel)
    {
        return new RelatorioDto(
            rel.Id,
            rel.Nome,
            rel.Soat_Analysis_Id,
            rel.IdDiagrama,            
            rel.Componentes_Identificado,
            rel.Risco_Arquitetura,
            rel.Recomendacao,
            rel.Message_Error
        );
    }
}

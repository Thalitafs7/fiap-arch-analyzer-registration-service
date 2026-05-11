using Application.Mappings;
using Domain.Entities;
using FsCheck;
using FsCheck.Xunit;
using UnitTests.Generators;

namespace UnitTests.Application.Mappings;

/// <summary>
/// Property-based tests for AnaliseServicoMappings extension methods.
/// Validates: Requirements 6.1, 6.2, 6.3, 6.4
/// </summary>
public class AnaliseServicoMappingsPropertyTests
{
    /// <summary>
    /// Property 6 (Req 6.1): ToDto() preserves all scalar fields of Analise.
    /// For any valid Analise, AnaliseDto.ClienteId, Id, Nome, Status, Descricao, DataCadastro match source.
    /// **Validates: Requirements 6.1**
    /// </summary>
    [Fact]
    public void Analise_ToDto_PreservesAllScalarFields()
    {
        Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            analise =>
            {
                var dto = analise.ToDto();

                return (dto.ClienteId == analise.ClienteId
                    && dto.Id == analise.Id
                    && dto.Nome == analise.Nome
                    && dto.Status == analise.Status.ToString()
                    && dto.Descricao == analise.Descricao
                    && dto.DataCadastro == analise.DataCadastro)
                    .ToProperty();
            }).QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// Property 6 (Req 6.2): ToDto() preserves Diagramas count.
    /// For any Analise with N Diagramas, AnaliseDto.Diagramas contains exactly N DiagramaDto items.
    /// **Validates: Requirements 6.2**
    /// </summary>
    [Fact]
    public void Analise_ToDto_PreservesDiagramasCount()
    {
        Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            analise =>
            {
                var dto = analise.ToDto();

                return (dto.Diagramas.Count == analise.Diagramas.Count)
                    .ToProperty();
            }).QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// Property 6 (Req 6.3): Diagrama.ToDto() preserves all fields.
    /// For any valid Diagrama, DiagramaDto.Id, Nome, TipoDiagrama, URLS3Diagrama match source.
    /// **Validates: Requirements 6.3**
    /// </summary>
    [Fact]
    public void Diagrama_ToDto_PreservesAllFields()
    {
        Prop.ForAll(
            DomainGenerators.ValidDiagrama(),
            diagrama =>
            {
                var dto = diagrama.ToDto();

                return (dto.Id == diagrama.Id
                    && dto.Nome == diagrama.Nome
                    && dto.TipoDiagrama == diagrama.TipoDiagrama
                    && dto.URLS3Diagrama == diagrama.URLS3Diagrama)
                    .ToProperty();
            }).QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// Property 6 (Req 6.4): Relatorio.ToDto() preserves all fields.
    /// For any valid Relatorio, RelatorioDto fields match source entity.
    /// **Validates: Requirements 6.4**
    /// </summary>
    [Fact]
    public void Relatorio_ToDto_PreservesAllFields()
    {
        Prop.ForAll(
            DomainGenerators.ValidRelatorio(),
            relatorio =>
            {
                var dto = relatorio.ToDto();

                return (dto.Id == relatorio.Id
                    && dto.Nome == relatorio.Nome
                    && dto.Soat_Analysis_Id == relatorio.Soat_Analysis_Id
                    && dto.IdDiagrama == relatorio.IdDiagrama
                    && dto.Componentes_Identificado == relatorio.Componentes_Identificado
                    && dto.Risco_Arquitetura == relatorio.Risco_Arquitetura
                    && dto.Recomendacao == relatorio.Recomendacao)
                    .ToProperty();
            }).QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// Property 6 (Req 6.2 + 6.3): Diagrama DTOs inside Analise.ToDto() preserve fields.
    /// Each DiagramaDto in AnaliseDto.Diagramas matches the corresponding source Diagrama.
    /// **Validates: Requirements 6.2, 6.3**
    /// </summary>
    [Fact]
    public void Analise_ToDto_DiagramaDtos_PreserveFields()
    {
        Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            analise =>
            {
                var dto = analise.ToDto();

                var allMatch = analise.Diagramas
                    .Zip(dto.Diagramas, (src, d) =>
                        d.Id == src.Id
                        && d.Nome == src.Nome
                        && d.TipoDiagrama == src.TipoDiagrama
                        && d.URLS3Diagrama == src.URLS3Diagrama)
                    .All(x => x);

                return allMatch.ToProperty();
            }).QuickCheckThrowOnFailure();
    }
}

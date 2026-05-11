using Domain.Entities;
using Domain.Exceptions;
using FsCheck;
using FsCheck.Xunit;
using UnitTests.Generators;

namespace UnitTests.Domain.Entities;

/// <summary>
/// Property-Based Tests for Relatorio entity.
/// Feature: registration-service-pbt-cleanup
/// </summary>
public class RelatorioPropertyTests
{
    #region Property 8: Relatorio valid construction and null-list initialization

    /// <summary>
    /// **Validates: Requirements 5.1**
    /// For any valid Nome (non-whitespace), valid IdDiagrama (≠ Empty), and any list params →
    /// Relatorio constructed with all properties correctly assigned.
    /// </summary>
    [Fact]
    public void Relatorio_ValidInputs_ConstructsCorrectly()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidRelatorio(),
            (Relatorio relatorio) =>
            {
                return relatorio.Nome != null
                    && !string.IsNullOrWhiteSpace(relatorio.Nome)
                    && relatorio.IdDiagrama != null
                    && relatorio.IdDiagrama != Guid.Empty
                    && relatorio.Componentes_Identificado != null
                    && relatorio.Risco_Arquitetura != null
                    && relatorio.Recomendacao != null;
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// **Validates: Requirements 5.4**
    /// When null passed for Componentes_Identificado, Risco_Arquitetura, or Recomendacao →
    /// initialized as empty lists (not null).
    /// </summary>
    [Fact]
    public void Relatorio_NullLists_InitializedAsEmptyLists()
    {
        var arb = (from nome in Arb.Generate<NonEmptyString>().Select(s => s.Get).Where(s => !string.IsNullOrWhiteSpace(s))
                   from soatId in Arb.Generate<Guid>()
                   from diagramaId in Arb.Generate<Guid>().Where(g => g != Guid.Empty)
                   select (nome, soatId, diagramaId)).ToArbitrary();

        var prop = Prop.ForAll(
            arb,
            (tuple) =>
            {
                var (nome, soatId, diagramaId) = tuple;

                var relatorio = new Relatorio(nome, soatId, diagramaId, null!, null!, null!);

                return relatorio.Componentes_Identificado != null
                    && relatorio.Componentes_Identificado.Count == 0
                    && relatorio.Risco_Arquitetura != null
                    && relatorio.Risco_Arquitetura.Count == 0
                    && relatorio.Recomendacao != null
                    && relatorio.Recomendacao.Count == 0;
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// **Validates: Requirements 5.2**
    /// For any whitespace Nome → construction throws DomainException.
    /// </summary>
    [Fact]
    public void Relatorio_WhitespaceNome_ThrowsDomainException()
    {
        var arb = (from whitespace in DomainGenerators.WhitespaceString().Generator
                   from soatId in Arb.Generate<Guid>()
                   from diagramaId in Arb.Generate<Guid>().Where(g => g != Guid.Empty)
                   select (whitespace, soatId, diagramaId)).ToArbitrary();

        var prop = Prop.ForAll(
            arb,
            (tuple) =>
            {
                var (whitespace, soatId, diagramaId) = tuple;

                try
                {
                    _ = new Relatorio(whitespace, soatId, diagramaId,
                        new List<string>(), new List<string>(), new List<string>());
                    return false; // should have thrown
                }
                catch (DomainException)
                {
                    return true;
                }
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// **Validates: Requirements 5.3**
    /// For Guid.Empty as IdDiagrama → construction throws DomainException.
    /// </summary>
    [Fact]
    public void Relatorio_EmptyIdDiagrama_ThrowsDomainException()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            (string nome) =>
            {
                try
                {
                    _ = new Relatorio(nome, Guid.NewGuid(), Guid.Empty,
                        new List<string>(), new List<string>(), new List<string>());
                    return false; // should have thrown
                }
                catch (DomainException)
                {
                    return true;
                }
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion
}

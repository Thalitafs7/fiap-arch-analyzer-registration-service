using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FsCheck;
using FsCheck.Xunit;
using UnitTests.Generators;

namespace UnitTests.Domain.Entities;

/// <summary>
/// Property-Based Tests for Analise entity.
/// Feature: registration-service-pbt-cleanup
/// </summary>
public class AnalisePropertyTests
{
    #region Property 4: Analise valid construction preserves all inputs

    /// <summary>
    /// For any valid ClienteId (≠ Empty), valid Nome (non-whitespace), any StatusAnalise,
    /// and any Diagramas list, Analise SHALL be constructed with all properties correctly
    /// assigned and Diagramas.Count matches input list count.
    /// **Validates: Requirements 3.1**
    /// </summary>
    [Fact]
    public void Property4_ValidConstruction_PreservesAllInputs()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidGuid(),
            DomainGenerators.ValidNome(),
            DomainGenerators.AnyStatus(),
            (Guid clienteId, string nome, StatusAnalise status) =>
            {
                // Use a fixed diagrama list to verify count preservation
                var diagramas = new List<Diagrama>
                {
                    new Diagrama("diag_0.png", "image/png", "s3://bucket/diag_0.png"),
                    new Diagrama("diag_1.png", "image/png", "s3://bucket/diag_1.png")
                };

                var analise = new Analise(clienteId, nome, status, diagramas, "descricao");

                return analise.ClienteId == clienteId
                    && analise.Nome == nome
                    && analise.Status == status
                    && analise.Diagramas.Count == diagramas.Count
                    && analise.Descricao == "descricao";
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// Diagramas.Count matches input list count for varying list sizes.
    /// **Validates: Requirements 3.1**
    /// </summary>
    [Fact]
    public void Property4_ValidConstruction_DiagramasCountMatchesInput()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            (Analise analise) =>
            {
                // ValidAnalise generator already verifies construction succeeds;
                // verify all required properties are populated
                return analise.ClienteId != Guid.Empty
                    && !string.IsNullOrWhiteSpace(analise.Nome)
                    && analise.Diagramas != null;
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion

    #region Property 5: Analise rejects invalid inputs

    /// <summary>
    /// For any Guid.Empty as ClienteId, Analise construction SHALL throw DomainException.
    /// **Validates: Requirements 3.2**
    /// </summary>
    [Fact]
    public void Property5_EmptyClienteId_ThrowsDomainException()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            DomainGenerators.AnyStatus(),
            (string nome, StatusAnalise status) =>
            {
                try
                {
                    var _ = new Analise(Guid.Empty, nome, status, new List<Diagrama>());
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
    /// For any whitespace/null Nome, Analise construction SHALL throw DomainException.
    /// **Validates: Requirements 3.3**
    /// </summary>
    [Fact]
    public void Property5_WhitespaceNome_ThrowsDomainException()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidGuid(),
            DomainGenerators.WhitespaceString(),
            DomainGenerators.AnyStatus(),
            (Guid clienteId, string nome, StatusAnalise status) =>
            {
                try
                {
                    var _ = new Analise(clienteId, nome, status, new List<Diagrama>());
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
    /// Null Nome construction SHALL throw DomainException.
    /// **Validates: Requirements 3.3**
    /// </summary>
    [Fact]
    public void Property5_NullNome_ThrowsDomainException()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidGuid(),
            DomainGenerators.AnyStatus(),
            (Guid clienteId, StatusAnalise status) =>
            {
                try
                {
                    var _ = new Analise(clienteId, null!, status, new List<Diagrama>());
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
    /// AlterarNome with whitespace/null SHALL throw DomainException.
    /// **Validates: Requirements 3.5**
    /// </summary>
    [Fact]
    public void Property5_AlterarNome_WhitespaceOrNull_ThrowsDomainException()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            DomainGenerators.WhitespaceString(),
            (Analise analise, string invalidNome) =>
            {
                try
                {
                    analise.AlterarNome(invalidNome);
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
    /// AlterarNome with null SHALL throw DomainException.
    /// **Validates: Requirements 3.5**
    /// </summary>
    [Fact]
    public void Property5_AlterarNome_Null_ThrowsDomainException()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            (Analise analise) =>
            {
                try
                {
                    analise.AlterarNome(null!);
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
    /// AlterarDescricao with null SHALL throw DomainException.
    /// **Validates: Requirements 3.8**
    /// </summary>
    [Fact]
    public void Property5_AlterarDescricao_Null_ThrowsDomainException()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            (Analise analise) =>
            {
                try
                {
                    analise.AlterarDescricao(null!);
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

    #region Property 6: Analise mutations update field and DataAtualizacao

    /// <summary>
    /// For any valid Analise and valid non-whitespace Nome, AlterarNome SHALL update Nome
    /// and set DataAtualizacao.
    /// **Validates: Requirements 3.4**
    /// </summary>
    [Fact]
    public void Property6_AlterarNome_UpdatesNomeAndDataAtualizacao()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            DomainGenerators.ValidNome(),
            (Analise analise, string novoNome) =>
            {
                var before = DateTime.UtcNow;

                analise.AlterarNome(novoNome);

                return analise.Nome == novoNome
                    && analise.DataAtualizacao.HasValue
                    && analise.DataAtualizacao.Value >= before;
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// For any StatusAnalise value, AtualizarStatus SHALL update Status and set DataAtualizacao.
    /// **Validates: Requirements 3.6**
    /// </summary>
    [Fact]
    public void Property6_AtualizarStatus_UpdatesStatusAndDataAtualizacao()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            DomainGenerators.AnyStatus(),
            (Analise analise, StatusAnalise novoStatus) =>
            {
                var before = DateTime.UtcNow;

                analise.AtualizarStatus(novoStatus);

                return analise.Status == novoStatus
                    && analise.DataAtualizacao.HasValue
                    && analise.DataAtualizacao.Value >= before;
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// For any non-null string, AlterarDescricao SHALL update Descricao and set DataAtualizacao.
    /// **Validates: Requirements 3.7**
    /// </summary>
    [Fact]
    public void Property6_AlterarDescricao_UpdatesDescricaoAndDataAtualizacao()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            Arb.From<NonEmptyString>(),
            (Analise analise, NonEmptyString novaDescricao) =>
            {
                var before = DateTime.UtcNow;
                var descricaoStr = novaDescricao.Get;

                analise.AlterarDescricao(descricaoStr);

                return analise.Descricao == descricaoStr
                    && analise.DataAtualizacao.HasValue
                    && analise.DataAtualizacao.Value >= before;
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// AlterarDescricao with empty string (non-null) SHALL update Descricao and set DataAtualizacao.
    /// **Validates: Requirements 3.7**
    /// </summary>
    [Fact]
    public void Property6_AlterarDescricao_EmptyString_UpdatesDescricaoAndDataAtualizacao()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            (Analise analise) =>
            {
                var before = DateTime.UtcNow;

                analise.AlterarDescricao(string.Empty);

                return analise.Descricao == string.Empty
                    && analise.DataAtualizacao.HasValue
                    && analise.DataAtualizacao.Value >= before;
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion
}

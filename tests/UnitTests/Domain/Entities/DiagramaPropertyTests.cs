using Domain.Entities;
using Domain.Exceptions;
using FsCheck;
using FsCheck.Xunit;
using UnitTests.Generators;

namespace UnitTests.Domain.Entities;

/// <summary>
/// Property-Based Tests for Diagrama entity.
/// Feature: registration-service-pbt-cleanup
///
/// **Validates: Requirements 4.1, 4.2, 4.3**
/// </summary>
public class DiagramaPropertyTests
{
    #region Property 7: Diagrama valid construction and rejection

    /// <summary>
    /// **Validates: Requirements 4.1**
    /// For any valid (non-empty) Nome and URLS3Diagrama → Diagrama constructed with all properties correctly assigned.
    /// </summary>
    [Fact]
    public void Diagrama_ValidInputs_ConstructsCorrectly()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidDiagrama(),
            (Diagrama diagrama) =>
            {
                return diagrama != null
                    && !string.IsNullOrEmpty(diagrama.Nome)
                    && !string.IsNullOrEmpty(diagrama.URLS3Diagrama)
                    && diagrama.Id != Guid.Empty
                    && diagrama.Ativo == true;
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// **Validates: Requirements 4.1**
    /// For any valid Nome, TipoDiagrama, and URLS3Diagrama → properties are assigned exactly as provided.
    /// </summary>
    [Fact]
    public void Diagrama_ValidInputs_AssignsAllPropertiesCorrectly()
    {
        var arb = (from nome in Arb.Generate<NonEmptyString>().Select(s => s.Get).Where(s => !string.IsNullOrWhiteSpace(s))
                   from tipo in Gen.Elements("image/png", "image/jpeg", "application/pdf")
                   from url in Arb.Generate<NonEmptyString>().Select(s => $"s3://bucket/{s.Get}")
                   select (nome, tipo, url)).ToArbitrary();

        var prop = Prop.ForAll(
            arb,
            (tuple) =>
            {
                var (nome, tipo, url) = tuple;
                var diagrama = new Diagrama(nome, tipo, url);

                return diagrama.Nome == nome
                    && diagrama.TipoDiagrama == tipo
                    && diagrama.URLS3Diagrama == url;
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// **Validates: Requirements 4.2**
    /// For any null or empty Nome → Diagrama construction throws DomainException.
    /// </summary>
    [Fact]
    public void Diagrama_NullOrEmptyNome_ThrowsDomainException()
    {
        var invalidNomes = new[] { null!, "", string.Empty };

        var prop = Prop.ForAll(
            Gen.Elements(invalidNomes).ToArbitrary(),
            (string invalidNome) =>
            {
                var validUrl = "s3://bucket/valid.png";
                try
                {
                    var _ = new Diagrama(invalidNome, "image/png", validUrl);
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
    /// **Validates: Requirements 4.3**
    /// For any null or empty URLS3Diagrama → Diagrama construction throws DomainException.
    /// </summary>
    [Fact]
    public void Diagrama_NullOrEmptyUrls3Diagrama_ThrowsDomainException()
    {
        var invalidUrls = new[] { null!, "", string.Empty };

        var prop = Prop.ForAll(
            Gen.Elements(invalidUrls).ToArbitrary(),
            (string invalidUrl) =>
            {
                var validNome = "valid-diagram.png";
                try
                {
                    var _ = new Diagrama(validNome, "image/png", invalidUrl);
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

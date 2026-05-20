using Domain.Enums;
using FsCheck;
using FsCheck.Xunit;
using UnitTests.Generators;

namespace UnitTests.Domain.Entities;

/// <summary>
/// Property-Based Tests for StatusAnalise enum mapping.
/// Feature: registration-service-pbt-cleanup
///
/// **Validates: Requirements 6.1, 6.2, 6.3**
/// </summary>
public class StatusAnalisePropertyTests
{
    #region Property 9: Unknown external status maps to Error

    private static readonly HashSet<string> KnownStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "RECEIVED", "RECEBIDO", "PROCESSING", "EMPROCESSAMENTO", "EM_PROCESSAMENTO",
        "ANALYZED", "ANALISADO", "ERROR", "ERRO"
    };

    /// <summary>
    /// **Validates: Requirements 6.1**
    /// Known strings map to correct enum values.
    /// </summary>
    [Fact]
    public void FromExternalStatus_RECEIVED_MapsToRecebido()
    {
        Assert.Equal(StatusAnalise.Recebido, StatusAnaliseExtensions.FromExternalStatus("RECEIVED"));
    }

    [Fact]
    public void FromExternalStatus_PROCESSING_MapsToEmProcessamento()
    {
        Assert.Equal(StatusAnalise.EmProcessamento, StatusAnaliseExtensions.FromExternalStatus("PROCESSING"));
    }

    [Fact]
    public void FromExternalStatus_ANALYZED_MapsToAnalisado()
    {
        Assert.Equal(StatusAnalise.Analisado, StatusAnaliseExtensions.FromExternalStatus("ANALYZED"));
    }

    [Fact]
    public void FromExternalStatus_ERROR_MapsToError()
    {
        Assert.Equal(StatusAnalise.Error, StatusAnaliseExtensions.FromExternalStatus("ERROR"));
    }

    /// <summary>
    /// **Validates: Requirements 6.2**
    /// For any string NOT in known statuses → throws ArgumentException.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property FromExternalStatus_UnknownString_ThrowsArgumentException()
    {
        var unknownStringArb = Arb.Generate<NonEmptyString>()
            .Select(s => s.Get)
            .Where(s => !string.IsNullOrWhiteSpace(s) && !KnownStatuses.Contains(s.Trim()))
            .ToArbitrary();

        return Prop.ForAll(
            unknownStringArb,
            (string unknownStatus) =>
            {
                try
                {
                    StatusAnaliseExtensions.FromExternalStatus(unknownStatus);
                    return false; // Should have thrown
                }
                catch (ArgumentException)
                {
                    return true;
                }
            });
    }

    /// <summary>
    /// **Validates: Requirements 6.3**
    /// For null or whitespace → returns StatusAnalise.Error.
    /// </summary>
    [Fact]
    public void FromExternalStatus_NullOrWhitespace_ReturnsError()
    {
        var prop = Prop.ForAll(
            DomainGenerators.WhitespaceString(),
            (string whitespace) =>
            {
                var result = StatusAnaliseExtensions.FromExternalStatus(whitespace);
                return result == StatusAnalise.Error;
            });

        prop.QuickCheckThrowOnFailure();
    }

    [Fact]
    public void FromExternalStatus_Null_ReturnsError()
    {
        var result = StatusAnaliseExtensions.FromExternalStatus(null);
        Assert.Equal(StatusAnalise.Error, result);
    }

    #endregion
}

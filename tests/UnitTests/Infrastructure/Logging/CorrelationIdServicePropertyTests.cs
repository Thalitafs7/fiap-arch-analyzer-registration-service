using FsCheck;
using FsCheck.Xunit;
using Infrastructure.Logging;

namespace UnitTests.Infrastructure.Logging;

/// <summary>
/// Property-Based Tests for CorrelationIdService.
/// </summary>
public class CorrelationIdServicePropertyTests
{
    #region Property 11: CorrelationIdService set-then-get identity

    /// <summary>
    /// A newly created CorrelationIdService SHALL return a non-empty, non-null correlation ID.
    /// **Validates: Requirements 14.1**
    /// </summary>
    [Fact]
    public void Property11_NewInstance_GetCorrelationId_ReturnsNonEmpty()
    {
        var sut = new CorrelationIdService();
        var id = sut.GetCorrelationId();

        Assert.False(string.IsNullOrEmpty(id));
    }

    /// <summary>
    /// For any valid non-empty Guid, SetCorrelationId followed by GetCorrelationId SHALL return that Guid as string.
    /// **Validates: Requirements 14.2**
    /// </summary>
    [Fact]
    public void Property11_SetValidGuid_GetCorrelationId_ReturnsThatGuid()
    {
        var prop = Prop.ForAll(
            Arb.Generate<Guid>().Where(g => g != Guid.Empty).ToArbitrary(),
            (Guid guid) =>
            {
                var sut = new CorrelationIdService();
                sut.SetCorrelationId(guid);
                return sut.GetCorrelationId() == guid.ToString();
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// SetCorrelationId with Guid.Empty SHALL NOT change the stored correlation ID.
    /// **Validates: Requirements 14.3**
    /// </summary>
    [Fact]
    public void Property11_SetGuidEmpty_DoesNotChangeCorrelationId()
    {
        var sut = new CorrelationIdService();
        var before = sut.GetCorrelationId();

        sut.SetCorrelationId(Guid.Empty);

        Assert.Equal(before, sut.GetCorrelationId());
    }

    /// <summary>
    /// SetCorrelationId with null SHALL NOT change the stored correlation ID.
    /// **Validates: Requirements 14.4**
    /// </summary>
    [Fact]
    public void Property11_SetNull_DoesNotChangeCorrelationId()
    {
        var sut = new CorrelationIdService();
        var before = sut.GetCorrelationId();

        sut.SetCorrelationId(null);

        Assert.Equal(before, sut.GetCorrelationId());
    }

    #endregion
}

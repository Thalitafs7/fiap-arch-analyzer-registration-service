using System.Security.Cryptography;
using System.Text;
using FsCheck;
using FsCheck.Xunit;
using Infrastructure.Security;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UnitTests.Generators;

namespace UnitTests.Infrastructure.Security;

/// <summary>
/// Property-Based Tests for WebhookSignatureValidator.
/// Feature: registration-service-pbt-cleanup
/// </summary>
public class WebhookSignatureValidatorPropertyTests
{
    private static WebhookSignatureValidator CreateValidator()
    {
        var logger = Substitute.For<ILogger<WebhookSignatureValidator>>();
        return new WebhookSignatureValidator(logger);
    }

    #region Property 15: WebhookSignatureValidator accepts correct HMAC-SHA256

    /// <summary>
    /// For any valid (xRequestId, dataId, secret) and a current timestamp,
    /// computing the correct HMAC-SHA256 hash and formatting as ts={ts},v1={hash}
    /// SHALL cause ValidateSignature to return true.
    /// **Validates: Requirements 11.1**
    /// </summary>
    [Fact]
    public void Property15_CorrectHmac_ValidateSignatureReturnsTrue()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            DomainGenerators.ValidNome(),
            DomainGenerators.ValidSecret(),
            (string xRequestId, string dataId, string secret) =>
            {
                var validator = CreateValidator();
                var ts = WebhookTestHelper.CurrentTimestamp();
                var hash = WebhookTestHelper.ComputeCorrectSignature(dataId, xRequestId, ts, secret);
                var xSignature = WebhookTestHelper.BuildXSignature(ts, hash);

                return validator.ValidateSignature(xSignature, xRequestId, dataId, secret);
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion

    #region Property 16: WebhookSignatureValidator rejects null/empty parameters

    /// <summary>
    /// For any combination where at least one of (xSignature, xRequestId, dataId, secret)
    /// is null or empty, ValidateSignature SHALL return false.
    /// **Validates: Requirements 11.2**
    /// </summary>
    [Fact]
    public void Property16_NullOrEmptyParameter_ValidateSignatureReturnsFalse()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            DomainGenerators.ValidNome(),
            DomainGenerators.ValidSecret(),
            (string xRequestId, string dataId, string secret) =>
            {
                var validator = CreateValidator();
                var ts = WebhookTestHelper.CurrentTimestamp();
                var hash = WebhookTestHelper.ComputeCorrectSignature(dataId, xRequestId, ts, secret);
                var validSignature = WebhookTestHelper.BuildXSignature(ts, hash);

                // Each null/empty variant must return false
                var nullSignature = !validator.ValidateSignature(null!, xRequestId, dataId, secret);
                var emptySignature = !validator.ValidateSignature("", xRequestId, dataId, secret);
                var nullRequestId = !validator.ValidateSignature(validSignature, null!, dataId, secret);
                var emptyRequestId = !validator.ValidateSignature(validSignature, "", dataId, secret);
                var nullDataId = !validator.ValidateSignature(validSignature, xRequestId, null!, secret);
                var emptyDataId = !validator.ValidateSignature(validSignature, xRequestId, "", secret);
                var nullSecret = !validator.ValidateSignature(validSignature, xRequestId, dataId, null!);
                var emptySecret = !validator.ValidateSignature(validSignature, xRequestId, dataId, "");

                return nullSignature && emptySignature
                    && nullRequestId && emptyRequestId
                    && nullDataId && emptyDataId
                    && nullSecret && emptySecret;
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion

    #region Property 17: WebhookSignatureValidator rejects incorrect hash

    /// <summary>
    /// For any valid parameters but with an xSignature containing a hash value
    /// that does NOT match the correct HMAC-SHA256 computation,
    /// ValidateSignature SHALL return false.
    /// **Validates: Requirements 11.3**
    /// </summary>
    [Fact]
    public void Property17_IncorrectHash_ValidateSignatureReturnsFalse()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            DomainGenerators.ValidNome(),
            DomainGenerators.ValidSecret(),
            (string xRequestId, string dataId, string secret) =>
            {
                var validator = CreateValidator();
                var ts = WebhookTestHelper.CurrentTimestamp();
                // Use a tampered hash: flip last char
                var correctHash = WebhookTestHelper.ComputeCorrectSignature(dataId, xRequestId, ts, secret);
                var tamperedHash = correctHash[..^1] + (correctHash[^1] == 'a' ? 'b' : 'a');
                var xSignature = WebhookTestHelper.BuildXSignature(ts, tamperedHash);

                return !validator.ValidateSignature(xSignature, xRequestId, dataId, secret);
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion

    #region Property 18: WebhookSignatureValidator rejects expired timestamp

    /// <summary>
    /// For any xSignature with a timestamp older than 5 minutes from current time,
    /// ValidateSignature SHALL return false regardless of hash correctness.
    /// **Validates: Requirements 11.4**
    /// </summary>
    [Fact]
    public void Property18_ExpiredTimestamp_ValidateSignatureReturnsFalse()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            DomainGenerators.ValidNome(),
            DomainGenerators.ValidSecret(),
            (string xRequestId, string dataId, string secret) =>
            {
                var validator = CreateValidator();
                var ts = WebhookTestHelper.ExpiredTimestamp();
                var hash = WebhookTestHelper.ComputeCorrectSignature(dataId, xRequestId, ts, secret);
                var xSignature = WebhookTestHelper.BuildXSignature(ts, hash);

                return !validator.ValidateSignature(xSignature, xRequestId, dataId, secret);
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion
}

/// <summary>
/// Helper for computing webhook signatures in tests.
/// </summary>
public static class WebhookTestHelper
{
    public static string ComputeCorrectSignature(string dataId, string xRequestId, string ts, string secret)
    {
        var manifest = $"id:{dataId};request-id:{xRequestId};ts:{ts};";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(manifest));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    public static string BuildXSignature(string ts, string hash) =>
        $"ts={ts},v1={hash}";

    public static string CurrentTimestamp() =>
        DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    public static string ExpiredTimestamp() =>
        DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeSeconds().ToString();
}

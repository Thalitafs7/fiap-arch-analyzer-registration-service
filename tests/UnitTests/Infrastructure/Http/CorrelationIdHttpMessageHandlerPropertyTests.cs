using Application.Common.Interfaces;
using FsCheck;
using FsCheck.Xunit;
using Infrastructure.Http;
using NSubstitute;
using System.Diagnostics;
using UnitTests.Generators;

namespace UnitTests.Infrastructure.Http;

/// <summary>
/// Property-Based Tests for CorrelationIdHttpMessageHandler.
/// </summary>
public class CorrelationIdHttpMessageHandlerPropertyTests
{
    /// <summary>
    /// Stub inner handler that captures the request and returns 200 OK.
    /// </summary>
    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }

    private static (HttpClient client, CapturingHandler inner) BuildClient(ICorrelationIdService correlationIdService)
    {
        var inner = new CapturingHandler();
        var handler = new CorrelationIdHttpMessageHandler(correlationIdService)
        {
            InnerHandler = inner
        };
        return (new HttpClient(handler), inner);
    }

    /// <summary>
    /// Property 13a: CorrelationIdHttpMessageHandler adds X-Correlation-ID when absent.
    ///
    /// For any HTTP request without X-Correlation-ID header, the handler SHALL add
    /// the header with the value from ICorrelationIdService.GetCorrelationId().
    ///
    /// **Validates: Requirements 16.1**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Handler_AddsCorrelationIdHeader_WhenAbsent()
    {
        var arb = DomainGenerators.ValidGuid();

        return Prop.ForAll(arb, correlationId =>
        {
            // Arrange
            var correlationIdService = Substitute.For<ICorrelationIdService>();
            correlationIdService.GetCorrelationId().Returns(correlationId.ToString());

            var (client, inner) = BuildClient(correlationIdService);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
            // No X-Correlation-ID header added

            // Act
            client.SendAsync(request).GetAwaiter().GetResult();

            // Assert
            var sentRequest = inner.LastRequest!;
            var hasHeader = sentRequest.Headers.TryGetValues("X-Correlation-ID", out var values);
            var headerValue = values?.FirstOrDefault();

            return (hasHeader && headerValue == correlationId.ToString()).Label(
                $"Expected X-Correlation-ID={correlationId}, got hasHeader={hasHeader}, value={headerValue}");
        });
    }

    /// <summary>
    /// Property 13b: CorrelationIdHttpMessageHandler does NOT overwrite existing X-Correlation-ID.
    ///
    /// For any request that already contains X-Correlation-ID header, the handler SHALL
    /// NOT overwrite the existing value.
    ///
    /// **Validates: Requirements 16.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Handler_DoesNotOverwrite_ExistingCorrelationIdHeader()
    {
        var arb = Gen.Two(Arb.Generate<Guid>().Where(g => g != Guid.Empty))
            .Where(pair => pair.Item1 != pair.Item2)
            .Select(pair => (pair.Item1, pair.Item2))
            .ToArbitrary();

        return Prop.ForAll(arb, tuple =>
        {
            var existingId = tuple.Item1;
            var serviceId = tuple.Item2;

            // Arrange
            var correlationIdService = Substitute.For<ICorrelationIdService>();
            correlationIdService.GetCorrelationId().Returns(serviceId.ToString());

            var (client, inner) = BuildClient(correlationIdService);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
            request.Headers.Add("X-Correlation-ID", existingId.ToString());

            // Act
            client.SendAsync(request).GetAwaiter().GetResult();

            // Assert: header value must remain the original, not the service value
            var sentRequest = inner.LastRequest!;
            sentRequest.Headers.TryGetValues("X-Correlation-ID", out var values);
            var headerValue = values?.FirstOrDefault();

            return (headerValue == existingId.ToString()).Label(
                $"Expected existing X-Correlation-ID={existingId} preserved, got={headerValue}");
        });
    }

    /// <summary>
    /// Property 13c: CorrelationIdHttpMessageHandler adds traceparent when Activity.Current != null.
    ///
    /// When SendAsync is called and Activity.Current is not null, the handler SHALL add
    /// a traceparent header if not already present.
    ///
    /// **Validates: Requirements 16.3**
    /// </summary>
    [Property(MaxTest = 50)]
    public Property Handler_AddsTraceparent_WhenActivityCurrentIsNotNull()
    {
        var arb = DomainGenerators.ValidGuid();

        return Prop.ForAll(arb, correlationId =>
        {
            // Arrange
            var correlationIdService = Substitute.For<ICorrelationIdService>();
            correlationIdService.GetCorrelationId().Returns(correlationId.ToString());

            var (client, inner) = BuildClient(correlationIdService);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
            // No traceparent header

            bool result;

            // Start an Activity so Activity.Current != null during SendAsync
            using var activity = new ActivitySource("test-source").StartActivity("test-operation")
                ?? new Activity("test-operation").Start();

            try
            {
                // Act
                client.SendAsync(request).GetAwaiter().GetResult();

                // Assert: traceparent header must be present
                var sentRequest = inner.LastRequest!;
                var hasTraceparent = sentRequest.Headers.Contains("traceparent");
                result = hasTraceparent;
            }
            finally
            {
                activity.Stop();
            }

            return result.Label("Expected traceparent header to be added when Activity.Current != null");
        });
    }

    /// <summary>
    /// Property 13d: CorrelationIdHttpMessageHandler does NOT add traceparent when Activity.Current is null.
    ///
    /// When SendAsync is called and Activity.Current is null, no traceparent header is added.
    /// </summary>
    [Property(MaxTest = 50)]
    public Property Handler_DoesNotAddTraceparent_WhenActivityCurrentIsNull()
    {
        var arb = DomainGenerators.ValidGuid();

        return Prop.ForAll(arb, correlationId =>
        {
            // Arrange — ensure no ambient activity
            Activity.Current = null;

            var correlationIdService = Substitute.For<ICorrelationIdService>();
            correlationIdService.GetCorrelationId().Returns(correlationId.ToString());

            var (client, inner) = BuildClient(correlationIdService);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

            // Act
            client.SendAsync(request).GetAwaiter().GetResult();

            // Assert
            var sentRequest = inner.LastRequest!;
            var hasTraceparent = sentRequest.Headers.Contains("traceparent");

            return (!hasTraceparent).Label("Expected no traceparent header when Activity.Current is null");
        });
    }
}

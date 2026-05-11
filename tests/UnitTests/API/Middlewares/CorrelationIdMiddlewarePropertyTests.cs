using API.Middlewares;
using Application.Common.Interfaces;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using UnitTests.Generators;

namespace UnitTests.API.Middlewares;

/// <summary>
/// Property-Based Tests for CorrelationIdMiddleware.
/// </summary>
public class CorrelationIdMiddlewarePropertyTests
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    /// <summary>
    /// IHttpResponseFeature implementation that fires OnStarting callbacks when StartAsync is called.
    /// DefaultHttpContext's built-in HttpResponseFeature does NOT fire callbacks in unit tests,
    /// so we replace it with this implementation.
    /// </summary>
    private sealed class CallbackAwareResponseFeature : IHttpResponseFeature
    {
        private readonly List<(Func<object, Task> callback, object state)> _onStartingCallbacks = new();

        public int StatusCode { get; set; } = 200;
        public string? ReasonPhrase { get; set; }
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public Stream Body { get; set; } = Stream.Null;
        public bool HasStarted { get; private set; }

        public void OnStarting(Func<object, Task> callback, object state)
            => _onStartingCallbacks.Add((callback, state));

        public void OnCompleted(Func<object, Task> callback, object state) { }

        public async Task FireOnStartingCallbacksAsync()
        {
            if (HasStarted) return;
            HasStarted = true;
            foreach (var (cb, state) in _onStartingCallbacks)
                await cb(state);
        }
    }

    /// <summary>
    /// Builds a DefaultHttpContext with the given ICorrelationIdService registered in RequestServices,
    /// and with a CallbackAwareResponseFeature so OnStarting callbacks fire correctly.
    /// Returns both the context and the response feature for callback triggering.
    /// </summary>
    private static (DefaultHttpContext context, CallbackAwareResponseFeature responseFeature) BuildContext(
        ICorrelationIdService correlationIdService,
        string? requestCorrelationId = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(correlationIdService);
        var provider = services.BuildServiceProvider();

        var responseFeature = new CallbackAwareResponseFeature();

        var context = new DefaultHttpContext();
        context.Features.Set<IHttpResponseFeature>(responseFeature);
        context.RequestServices = provider;

        if (requestCorrelationId is not null)
            context.Request.Headers[CorrelationIdHeader] = requestCorrelationId;

        return (context, responseFeature);
    }

    /// <summary>
    /// Property 15a: SetCorrelationId called with Guid from header.
    ///
    /// For any request with a valid X-Correlation-ID header containing a parseable Guid,
    /// the middleware SHALL call SetCorrelationId with that Guid.
    ///
    /// **Validates: Requirements 18.1**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Middleware_CallsSetCorrelationId_WithGuidFromHeader()
    {
        return Prop.ForAll(DomainGenerators.ValidGuid(), correlationId =>
        {
            // Arrange
            var correlationIdService = Substitute.For<ICorrelationIdService>();
            correlationIdService.GetCorrelationId().Returns(correlationId.ToString());

            var (context, _) = BuildContext(correlationIdService, correlationId.ToString());
            var middleware = new CorrelationIdMiddleware();

            // Act
            middleware.InvokeAsync(context, _ => Task.CompletedTask).GetAwaiter().GetResult();

            // Assert: SetCorrelationId must have been called with the exact Guid from the header
            correlationIdService.Received(1).SetCorrelationId(correlationId);

            return true.Label(
                $"Expected SetCorrelationId called with {correlationId}");
        });
    }

    /// <summary>
    /// Property 15b: SetCorrelationId NOT called when header is absent.
    ///
    /// When a request does not contain X-Correlation-ID header, the middleware SHALL NOT
    /// call SetCorrelationId (default ID is preserved).
    ///
    /// **Validates: Requirements 18.2**
    /// </summary>
    [Fact]
    public async Task Middleware_DoesNotCallSetCorrelationId_WhenHeaderAbsent()
    {
        // Arrange
        var correlationIdService = Substitute.For<ICorrelationIdService>();
        correlationIdService.GetCorrelationId().Returns(Guid.NewGuid().ToString());

        // No X-Correlation-ID header
        var (context, _) = BuildContext(correlationIdService, requestCorrelationId: null);
        var middleware = new CorrelationIdMiddleware();

        // Act
        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        // Assert: SetCorrelationId must NOT have been called
        correlationIdService.DidNotReceive().SetCorrelationId(Arg.Any<Guid?>());
    }

    /// <summary>
    /// Property 15b (PBT variant): SetCorrelationId NOT called for any non-parseable or absent header.
    ///
    /// For any string that is NOT a valid Guid, the middleware SHALL NOT call SetCorrelationId.
    ///
    /// **Validates: Requirements 18.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Middleware_DoesNotCallSetCorrelationId_WhenHeaderIsNotValidGuid()
    {
        // Generate non-empty strings that are NOT valid Guids
        var arb = Arb.Generate<NonEmptyString>()
            .Select(s => s.Get)
            .Where(s => !Guid.TryParse(s, out _))
            .ToArbitrary();

        return Prop.ForAll(arb, invalidHeader =>
        {
            // Arrange
            var correlationIdService = Substitute.For<ICorrelationIdService>();
            correlationIdService.GetCorrelationId().Returns(Guid.NewGuid().ToString());

            var (context, _) = BuildContext(correlationIdService, invalidHeader);
            var middleware = new CorrelationIdMiddleware();

            // Act
            middleware.InvokeAsync(context, _ => Task.CompletedTask).GetAwaiter().GetResult();

            // Assert
            correlationIdService.DidNotReceive().SetCorrelationId(Arg.Any<Guid?>());

            return true.Label(
                $"SetCorrelationId must not be called for non-Guid header: '{invalidHeader}'");
        });
    }

    /// <summary>
    /// Property 15c: Response always contains X-Correlation-ID header.
    ///
    /// The response SHALL always contain X-Correlation-ID header with the value
    /// from ICorrelationIdService.GetCorrelationId.
    ///
    /// **Validates: Requirements 18.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Middleware_ResponseContainsCorrelationIdHeader()
    {
        return Prop.ForAll(DomainGenerators.ValidGuid(), correlationId =>
        {
            // Arrange
            var correlationIdService = Substitute.For<ICorrelationIdService>();
            correlationIdService.GetCorrelationId().Returns(correlationId.ToString());

            // No header in request — tests that response header is always set regardless
            var (context, responseFeature) = BuildContext(correlationIdService, requestCorrelationId: null);
            var middleware = new CorrelationIdMiddleware();

            // Act: invoke middleware, then fire OnStarting callbacks manually
            middleware.InvokeAsync(context, _ => Task.CompletedTask).GetAwaiter().GetResult();
            responseFeature.FireOnStartingCallbacksAsync().GetAwaiter().GetResult();

            // Assert: response must contain X-Correlation-ID
            var hasHeader = responseFeature.Headers.ContainsKey(CorrelationIdHeader);
            var headerValue = responseFeature.Headers[CorrelationIdHeader].ToString();

            return (hasHeader && headerValue == correlationId.ToString()).Label(
                $"Expected response X-Correlation-ID={correlationId}, got hasHeader={hasHeader}, value='{headerValue}'");
        });
    }

    /// <summary>
    /// Property 15c (with header): Response contains X-Correlation-ID even when request had a valid header.
    ///
    /// **Validates: Requirements 18.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Middleware_ResponseContainsCorrelationIdHeader_WhenRequestHadHeader()
    {
        return Prop.ForAll(DomainGenerators.ValidGuid(), correlationId =>
        {
            // Arrange
            var correlationIdService = Substitute.For<ICorrelationIdService>();
            correlationIdService.GetCorrelationId().Returns(correlationId.ToString());

            // Request has a valid Guid header
            var (context, responseFeature) = BuildContext(correlationIdService, correlationId.ToString());
            var middleware = new CorrelationIdMiddleware();

            // Act
            middleware.InvokeAsync(context, _ => Task.CompletedTask).GetAwaiter().GetResult();
            responseFeature.FireOnStartingCallbacksAsync().GetAwaiter().GetResult();

            // Assert
            var hasHeader = responseFeature.Headers.ContainsKey(CorrelationIdHeader);
            var headerValue = responseFeature.Headers[CorrelationIdHeader].ToString();

            return (hasHeader && headerValue == correlationId.ToString()).Label(
                $"Expected response X-Correlation-ID={correlationId}, got hasHeader={hasHeader}, value='{headerValue}'");
        });
    }
}

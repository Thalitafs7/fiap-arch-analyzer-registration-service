using Application.Common.Interfaces;
using FsCheck;
using FsCheck.Xunit;
using Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UnitTests.Generators;

namespace UnitTests.Infrastructure.Logging;

/// <summary>
/// Property-Based Tests for LogService.
///
/// **Validates: Requirements 13.1, 13.2, 13.3, 13.4**
/// </summary>
public class LogServicePropertyTests
{
    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    private static (LogService<LogServicePropertyTests> sut,
                    ILogger<LogService<LogServicePropertyTests>> logger,
                    ICorrelationIdService correlationIdService)
        CreateSut(string correlationId = "test-correlation-id")
    {
        var logger = Substitute.For<ILogger<LogService<LogServicePropertyTests>>>();
        var correlationIdService = Substitute.For<ICorrelationIdService>();
        var currentUserService = Substitute.For<ICurrentUserService>();

        correlationIdService.GetCorrelationId().Returns(correlationId);
        currentUserService.UserName.Returns("test-user");

        var sut = new LogService<LogServicePropertyTests>(
            correlationIdService, logger, currentUserService);

        return (sut, logger, correlationIdService);
    }

    /// <summary>
    /// Extracts the formatted log message string from a received ILogger.Log call.
    /// ILogger extension methods call Log[TState] — we capture via ReceivedCalls()
    /// and invoke the formatter to get the final string.
    /// </summary>
    private static string GetLogPayload(ILogger logger, int callIndex = 0)
    {
        var calls = logger.ReceivedCalls().ToList();
        if (calls.Count <= callIndex) return string.Empty;

        var args = calls[callIndex].GetArguments();
        // args: [LogLevel, EventId, TState state, Exception?, Func<TState,Exception?,string> formatter]
        var state = args[2];
        var exception = args[3] as Exception;
        var formatter = args[4];

        // Invoke formatter(state, exception) to get the rendered string
        var formatterType = formatter!.GetType();
        var invokeMethod = formatterType.GetMethod("Invoke");
        return invokeMethod!.Invoke(formatter, new[] { state, exception }) as string ?? string.Empty;
    }

    // -------------------------------------------------------------------------
    // Property 13.1: LogInicio → LogInformation with method name and "Inicio"
    // -------------------------------------------------------------------------

    /// <summary>
    /// For any method name and props, LogInicio SHALL invoke ILogger.LogInformation
    /// with a JSON payload containing the method name and "Inicio" etapa.
    ///
    /// **Validates: Requirements 13.1**
    /// </summary>
    [Fact]
    public void LogInicio_AnyMethodName_LogsInformationWithMethodNameAndInicioEtapa()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            (string metodo) =>
            {
                // Arrange
                var (sut, logger, _) = CreateSut();

                // Act
                sut.LogInicio(metodo, new { Prop = "value" });

                // Assert: exactly 1 call at Information level
                logger.Received(1).Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Any<object>(),
                    Arg.Any<Exception?>(),
                    Arg.Any<Func<object, Exception?, string>>());

                // Assert: payload contains method name and "Inicio"
                var payload = GetLogPayload(logger);
                return (payload.Contains(metodo) || payload.Contains(System.Text.Json.JsonEncodedText.Encode(metodo).ToString()))
                    && payload.Contains("Inicio");
            });

        prop.QuickCheckThrowOnFailure();
    }

    // -------------------------------------------------------------------------
    // Property 13.2: LogFim → LogInformation with "Fim"
    // -------------------------------------------------------------------------

    /// <summary>
    /// For any method name and return object, LogFim SHALL invoke ILogger.LogInformation
    /// with a JSON payload containing "Fim" etapa.
    ///
    /// **Validates: Requirements 13.2**
    /// </summary>
    [Fact]
    public void LogFim_AnyMethodName_LogsInformationWithFimEtapa()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            (string metodo) =>
            {
                // Arrange
                var (sut, logger, _) = CreateSut();

                // Act
                sut.LogFim(metodo, new { Result = "ok" });

                // Assert: exactly 1 call at Information level
                logger.Received(1).Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Any<object>(),
                    Arg.Any<Exception?>(),
                    Arg.Any<Func<object, Exception?, string>>());

                // Assert: payload contains "Fim"
                var payload = GetLogPayload(logger);
                return payload.Contains("Fim");
            });

        prop.QuickCheckThrowOnFailure();
    }

    // -------------------------------------------------------------------------
    // Property 13.3: LogErro → LogError with exception and "Erro"
    // -------------------------------------------------------------------------

    /// <summary>
    /// For any method name and exception, LogErro SHALL invoke ILogger.LogError
    /// with the exception and a JSON payload containing "Erro" etapa.
    ///
    /// **Validates: Requirements 13.3**
    /// </summary>
    [Fact]
    public void LogErro_AnyMethodNameAndException_LogsErrorWithExceptionAndErroEtapa()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            DomainGenerators.AnyException(),
            (string metodo, Exception ex) =>
            {
                // Arrange
                var (sut, logger, _) = CreateSut();

                // Act
                sut.LogErro(metodo, ex);

                // Assert: exactly 1 call at Error level with the exception
                logger.Received(1).Log(
                    LogLevel.Error,
                    Arg.Any<EventId>(),
                    Arg.Any<object>(),
                    Arg.Is<Exception?>(e => e == ex),
                    Arg.Any<Func<object, Exception?, string>>());

                // Assert: payload contains "Erro"
                var payload = GetLogPayload(logger);
                return payload.Contains("Erro");
            });

        prop.QuickCheckThrowOnFailure();
    }

    // -------------------------------------------------------------------------
    // Property 13.4: CorrelationId included in every log entry
    // -------------------------------------------------------------------------

    /// <summary>
    /// LogService SHALL include the CorrelationId from ICorrelationIdService in every
    /// log entry (LogInicio, LogFim, LogErro).
    ///
    /// **Validates: Requirements 13.4**
    /// </summary>
    [Fact]
    public void AllLogMethods_IncludeCorrelationIdInPayload()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            Arb.Generate<Guid>().Where(g => g != Guid.Empty).ToArbitrary(),
            (string metodo, Guid correlationGuid) =>
            {
                var correlationId = correlationGuid.ToString();

                // --- LogInicio ---
                var (sut1, logger1, _) = CreateSut(correlationId);
                sut1.LogInicio(metodo);
                var payload1 = GetLogPayload(logger1);

                // --- LogFim ---
                var (sut2, logger2, _) = CreateSut(correlationId);
                sut2.LogFim(metodo);
                var payload2 = GetLogPayload(logger2);

                // --- LogErro ---
                var (sut3, logger3, _) = CreateSut(correlationId);
                sut3.LogErro(metodo, new Exception("test"));
                var payload3 = GetLogPayload(logger3);

                return payload1.Contains(correlationId)
                    && payload2.Contains(correlationId)
                    && payload3.Contains(correlationId);
            });

        prop.QuickCheckThrowOnFailure();
    }
}

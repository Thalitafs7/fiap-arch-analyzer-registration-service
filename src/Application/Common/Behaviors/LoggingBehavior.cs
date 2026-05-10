using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Application.Common.Behaviors;

[ExcludeFromCodeCoverage]
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICorrelationIdService correlationIdService,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _correlationIdService = correlationIdService;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = _correlationIdService.GetCorrelationId();
        var userId = _currentUserService.UserId ?? "anonymous";
        var activity = Activity.Current;

        _logger.LogInformation(
            "[Inicio] {RequestName} | CorrelationId: {CorrelationId} | Usuario: {Usuario} | Request: {Request}",
            requestName,
            correlationId,
            userId,
            SerializeSafe(request));

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation(
                "[Fim] {RequestName} | CorrelationId: {CorrelationId} | Duracao: {ElapsedMs}ms | Response: {Response}",
                requestName,
                correlationId,
                stopwatch.ElapsedMilliseconds,
                SerializeSafe(response));

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "[Erro] {RequestName} | CorrelationId: {CorrelationId} | Duracao: {ElapsedMs}ms | Erro: {ErrorMessage}",
                requestName,
                correlationId,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            throw;
        }
    }

    private static string SerializeSafe(object? obj)
    {
        if (obj == null) return "null";

        try
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = false,
                MaxDepth = 3,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            });
        }
        catch
        {
            return obj.GetType().Name;
        }
    }
}

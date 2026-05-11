using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Infrastructure.Logging;

public class LogService<T> : ILogService<T>
{
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LogService<T>> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public LogService(
        ICorrelationIdService correlationIdService,
        ILogger<LogService<T>> logger,
        ICurrentUserService currentUserService)
    {
        _correlationIdService = correlationIdService;
        _logger = logger;
        _currentUserService = currentUserService;

        _jsonOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = false
        };
    }

    public void LogInicio(string metodo, object? props = null)
        => LogGeneric(LogLevel.Information, metodo, "Inicio", props);

    public void LogFim(string metodo, object? retorno = null)
        => LogGeneric(LogLevel.Information, metodo, "Fim", retorno);

    public void LogErro(string metodo, Exception ex)
    {
        var dados = new
        {
            Mensagem = ex.Message,
            Tipo = ex.GetType().Name,
            ex.StackTrace
        };

        LogGeneric(LogLevel.Error, metodo, "Erro", dados, ex);
    }

    private void LogGeneric(
        LogLevel nivel,
        string metodo,
        string etapa,
        object? dados,
        Exception? exception = null)
    {
        var activity = Activity.Current;
        var entry = new LogEntryDto
        {
            Nivel = nivel.ToString(),
            Classe = typeof(T).Name,
            Metodo = metodo,
            Etapa = etapa,
            CorrelationId = _correlationIdService.GetCorrelationId(),
            TraceId = activity?.TraceId.ToString(),
            SpanId = activity?.SpanId.ToString(),
            Dados = dados,
            Timestamp = DateTime.UtcNow,
            Usuario = _currentUserService.UserName
        };

        var payload = JsonSerializer.Serialize(entry, _jsonOptions);

        if (nivel == LogLevel.Error)
            _logger.LogError(exception, "{LogEntry}", payload);
        else
            _logger.LogInformation("{LogEntry}", payload);
    }
}

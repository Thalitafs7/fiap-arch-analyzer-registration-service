using Application.Common.Interfaces;

namespace Application.Common.Handlers;

public abstract class QueryHandlerBase<THandler> where THandler : class
{
    protected readonly ILogService<THandler> _logService;

    protected QueryHandlerBase(ILogService<THandler> logService)
    {
        _logService = logService;
    }

    protected void LogInicio(string metodo, object? props = null)
        => _logService.LogInicio(metodo, props);

    protected void LogFim(string metodo, object? retorno = null)
        => _logService.LogFim(metodo, retorno);

    protected void LogErro(string metodo, Exception ex)
        => _logService.LogErro(metodo, ex);
}

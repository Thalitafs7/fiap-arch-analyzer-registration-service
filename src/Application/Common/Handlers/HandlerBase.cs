using Application.Common.Interfaces;
using Domain.Interfaces;

namespace Application.Common.Handlers;

public abstract class HandlerBase<THandler> where THandler : class
{
    protected readonly ILogService<THandler> _logService;
    protected readonly IUnitOfWork _unitOfWork;

    protected HandlerBase(ILogService<THandler> logService, IUnitOfWork unitOfWork)
    {
        _logService = logService;
        _unitOfWork = unitOfWork;
    }

    protected void LogInicio(string metodo, object? props = null)
        => _logService.LogInicio(metodo, props);

    protected void LogFim(string metodo, object? retorno = null)
        => _logService.LogFim(metodo, retorno);

    protected void LogErro(string metodo, Exception ex)
        => _logService.LogErro(metodo, ex);

    protected async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.CommitAsync(cancellationToken);
    }
}

using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IErrorRepository _errorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingBehavior(
        IErrorRepository errorRepository,
        IUnitOfWork unitOfWork,
        ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    {
        _errorRepository = errorRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestTypeName = typeof(TRequest).Name;

            _logger.LogError(ex, "[ExceptionHandlingBehavior] Error in {RequestName}", requestTypeName);

            var error = new Error(
                Guid.NewGuid(),
                requestTypeName,
                ex.GetType().ToString() + " - " +
                ex.Message +
                ex.StackTrace +
                ex.Source);

            await _errorRepository.AdicionarAsync(error, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            throw;
        }
    }
}

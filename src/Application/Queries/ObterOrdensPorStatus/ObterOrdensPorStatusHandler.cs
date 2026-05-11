using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Interfaces;
using MediatR;

namespace Application.Queries.ObterOrdensPorStatus;

public class ObterOrdensPorStatusHandler : QueryHandlerBase<ObterOrdensPorStatusHandler>, IRequestHandler<ObterOrdensPorStatusQuery, IEnumerable<AnaliseDto>>
{
    private readonly IAnaliseRepository _ordemRepository;

    public ObterOrdensPorStatusHandler(
        IAnaliseRepository ordemRepository,
        ILogService<ObterOrdensPorStatusHandler> logService)
        : base(logService)
    {
        _ordemRepository = ordemRepository;
    }

    public async Task<IEnumerable<AnaliseDto>> Handle(ObterOrdensPorStatusQuery query, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        try
        {
            LogInicio(metodo, query);

            var ordens = await _ordemRepository.ObterTodosAsync(cancellationToken);
            var resultado = ordens.Select(o => o.ToDto()).ToList();

            LogFim(metodo, new { Count = resultado.Count });
            return resultado;
        }
        catch (Exception ex)
        {
            LogErro(metodo, ex);
            throw;
        }
    }
}

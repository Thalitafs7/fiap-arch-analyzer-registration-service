using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Interfaces;
using MediatR;

namespace Application.Queries.ObterTodasOrdens;

public class ObterTodasOrdensHandler : QueryHandlerBase<ObterTodasOrdensHandler>, IRequestHandler<ObterTodasOrdensQuery, IEnumerable<AnaliseDto>>
{
    private readonly IAnaliseRepository _ordemRepository;

    public ObterTodasOrdensHandler(
        IAnaliseRepository ordemRepository,
        ILogService<ObterTodasOrdensHandler> logService)
        : base(logService)
    {
        _ordemRepository = ordemRepository;
    }

    public async Task<IEnumerable<AnaliseDto>> Handle(ObterTodasOrdensQuery query, CancellationToken cancellationToken)
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

using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Interfaces;
using MediatR;

namespace Application.Queries.ObterAnaliseAllServico;

public class ObterAnaliseAllServicoHandler : QueryHandlerBase<ObterAnaliseAllServicoHandler>, IRequestHandler<ObterAnaliseAllServicoQuery, IEnumerable<AnaliseDto>>
{
    private readonly IAnaliseRepository _analiseRepository;

    public ObterAnaliseAllServicoHandler(
        IAnaliseRepository analiseRepository,
        ILogService<ObterAnaliseAllServicoHandler> logService)
        : base(logService)
    {
        _analiseRepository = analiseRepository;
    }

    public async Task<IEnumerable<AnaliseDto?>> Handle(ObterAnaliseAllServicoQuery query, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        try
        {
            LogInicio(metodo, query);

            var analiseServico = await _analiseRepository.ObterTodosAsync(cancellationToken);
            

            var resultado = analiseServico.Select(o => o.ToDto()).ToList();

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

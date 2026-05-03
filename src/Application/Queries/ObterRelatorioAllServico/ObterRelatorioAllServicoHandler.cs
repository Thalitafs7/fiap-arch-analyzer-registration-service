using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Interfaces;
using MediatR;

namespace Application.Queries.ObterRelatorioAllServico;

public class ObterRelatorioAllServicoHandler : QueryHandlerBase<ObterRelatorioAllServicoHandler>, IRequestHandler<ObterRelatorioAllServicoQuery, IEnumerable<RelatorioDto>>
{
    private readonly IRelatorioRepository _relatorioRepository;

    public ObterRelatorioAllServicoHandler(
        IRelatorioRepository relatorioRepository,
        ILogService<ObterRelatorioAllServicoHandler> logService)
        : base(logService)
    {
        _relatorioRepository = relatorioRepository;
    }

    public async Task<IEnumerable<RelatorioDto?>> Handle(ObterRelatorioAllServicoQuery query, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        try
        {
            LogInicio(metodo, query);

            var analiseServico = await _relatorioRepository.ObterTodosAsync(cancellationToken);


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

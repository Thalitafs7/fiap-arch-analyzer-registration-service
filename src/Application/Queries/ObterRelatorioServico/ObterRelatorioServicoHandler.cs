using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Interfaces;
using MediatR;

namespace Application.Queries.ObterRelatorioServico;

public class ObterRelatorioServicoHandler : QueryHandlerBase<ObterRelatorioServicoHandler>, IRequestHandler<ObterRelatorioServicoQuery, RelatorioDto?>
{
    private readonly IRelatorioRepository _relatorioRepository;

    public ObterRelatorioServicoHandler(
        IRelatorioRepository relatorioRepository,
        ILogService<ObterRelatorioServicoHandler> logService)
        : base(logService)
    {
        _relatorioRepository = relatorioRepository;
    }

    public async Task<RelatorioDto?> Handle(ObterRelatorioServicoQuery query, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        try
        {
            LogInicio(metodo, query);

            var relatorioServico = await _relatorioRepository.ObterPorIdAsync(query.Id, cancellationToken);
            var resultado = relatorioServico?.ToDto();

            LogFim(metodo, resultado);
            return resultado;
        }
        catch (Exception ex)
        {
            LogErro(metodo, ex);
            throw;
        }
    }
}

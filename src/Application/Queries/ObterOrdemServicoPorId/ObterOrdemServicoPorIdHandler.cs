using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Interfaces;
using MediatR;

namespace Application.Queries.ObterOrdemServicoPorId;

public class ObterOrdemServicoPorIdHandler : QueryHandlerBase<ObterOrdemServicoPorIdHandler>, IRequestHandler<ObterOrdemServicoPorIdQuery, AnaliseDto?>
{
    private readonly IAnaliseRepository _ordemRepository;

    public ObterOrdemServicoPorIdHandler(
        IAnaliseRepository ordemRepository,
        ILogService<ObterOrdemServicoPorIdHandler> logService)
        : base(logService)
    {
        _ordemRepository = ordemRepository;
    }

    public async Task<AnaliseDto?> Handle(ObterOrdemServicoPorIdQuery query, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        try
        {
            LogInicio(metodo, query);

            var ordemServico = await _ordemRepository.ObterPorOrdemServicoIdAsync(query.Id, cancellationToken);
            var resultado = ordemServico?.ToDto();

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

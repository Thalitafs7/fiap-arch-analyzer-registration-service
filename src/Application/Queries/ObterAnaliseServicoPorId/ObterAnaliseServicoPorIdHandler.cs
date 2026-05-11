using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Interfaces;
using MediatR;

namespace Application.Queries.ObterAnaliseServicoPorId;

public class ObterAnaliseServicoPorIdHandler : QueryHandlerBase<ObterAnaliseServicoPorIdHandler>, IRequestHandler<ObterAnaliseServicoPorIdQuery, AnaliseDto?>
{
    private readonly IAnaliseRepository _analiseRepository;

    public ObterAnaliseServicoPorIdHandler(
        IAnaliseRepository analiseRepository,
        ILogService<ObterAnaliseServicoPorIdHandler> logService)
        : base(logService)
    {
        _analiseRepository = analiseRepository;
    }

    public async Task<AnaliseDto?> Handle(ObterAnaliseServicoPorIdQuery query, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        try
        {
            LogInicio(metodo, query);

            var ordemServico = await _analiseRepository.ObterPorOrdemServicoIdAsync(query.Id, cancellationToken);
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

using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Interfaces;
using MediatR;

namespace Application.Queries.UpdateAnaliseServico;

public class UpdateAnaliseServicoHandler : QueryHandlerBase<UpdateAnaliseServicoHandler>, IRequestHandler<UpdateAnaliseServicoQuery, AnaliseDto?>
{
    private readonly IAnaliseRepository _analiseRepository;

    public UpdateAnaliseServicoHandler(
        IAnaliseRepository analiseRepository,
        ILogService<UpdateAnaliseServicoHandler> logService)
        : base(logService)
    {
        _analiseRepository = analiseRepository;
    }

    public async Task<AnaliseDto?> Handle(UpdateAnaliseServicoQuery query, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        try
        {
            LogInicio(metodo, query);

            var analise = await _analiseRepository.ObterPorIdAsync(query.Id, cancellationToken);

            if (analise is null)
                throw new Exception("Analise não existe");            

            analise.Status = "Em Processamento"; // Exemplo de atualização, ajuste conforme necessário

            _analiseRepository.Atualizar(analise);
            var resultado = analise?.ToDto();

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

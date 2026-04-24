using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Interfaces;
using MediatR;

namespace Application.Queries.ObterOrdensAtivas;

public class ObterOrdensAtivasHandler : QueryHandlerBase<ObterOrdensAtivasHandler>, IRequestHandler<ObterOrdensAtivasQuery, IEnumerable<AnaliseDto>>
{
    private readonly IAnaliseRepository _ordemRepository;

    public ObterOrdensAtivasHandler(
        IAnaliseRepository ordemRepository,
        ILogService<ObterOrdensAtivasHandler> logService)
        : base(logService)
    {
        _ordemRepository = ordemRepository;
    }

    public async Task<IEnumerable<AnaliseDto>> Handle(ObterOrdensAtivasQuery query, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        try
        {
            LogInicio(metodo, query);

            var todasOrdens = await _ordemRepository.ObterTodosAsync(cancellationToken);

            var statusAtivos = new[]
            {
                "Em Análise",
                "Aguardando Aprovação",
                "Aprovada",
                "Em Execução"

            };

            var ordensAtivas = todasOrdens
                .Where(o => statusAtivos.Contains(o.Nome))
                .Select(o => o.ToDto())
                .ToList();

            LogFim(metodo, new { Count = ordensAtivas.Count });
            return ordensAtivas;
        }
        catch (Exception ex)
        {
            LogErro(metodo, ex);
            throw;
        }
    }
}

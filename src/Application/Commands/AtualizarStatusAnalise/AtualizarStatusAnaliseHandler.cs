using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Interfaces;
using MediatR;

namespace Application.Commands.AtualizarStatusAnalise;

public class AtualizarStatusAnaliseHandler : HandlerBase<AtualizarStatusAnaliseHandler>, IRequestHandler<AtualizarStatusAnaliseCommand, AnaliseDto?>
{
    private readonly IAnaliseRepository _analiseRepository;

    public AtualizarStatusAnaliseHandler(
        IAnaliseRepository analiseRepository,
        IUnitOfWork unitOfWork,
        ILogService<AtualizarStatusAnaliseHandler> logService)
        : base(logService, unitOfWork)
    {
        _analiseRepository = analiseRepository;
    }

    public async Task<AnaliseDto?> Handle(AtualizarStatusAnaliseCommand command, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        try
        {
            LogInicio(metodo, command);

            var analise = await _analiseRepository.ObterPorIdAsync(command.Id, cancellationToken);

            if (analise is null)
                throw new Exception("Analise não existe");

            analise.AtualizarStatus(Domain.Enums.StatusAnalise.EmProcessamento);

            if (command.SoatAnalysisId.HasValue && command.SoatAnalysisId.Value != Guid.Empty)
                analise.AtualizarSoatAnalysisId(command.SoatAnalysisId.Value);

            _analiseRepository.Atualizar(analise);

            await CommitAsync(cancellationToken);

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

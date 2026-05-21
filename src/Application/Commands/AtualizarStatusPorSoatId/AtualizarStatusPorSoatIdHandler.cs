using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Commands.AtualizarStatusPorSoatId;

public class AtualizarStatusPorSoatIdHandler : HandlerBase<AtualizarStatusPorSoatIdHandler>, IRequestHandler<AtualizarStatusPorSoatIdCommand, AnaliseDto>
{
    private readonly IAnaliseRepository _analiseRepository;

    public AtualizarStatusPorSoatIdHandler(
        IAnaliseRepository analiseRepository,
        IUnitOfWork unitOfWork,
        ILogService<AtualizarStatusPorSoatIdHandler> logService)
        : base(logService, unitOfWork)
    {
        _analiseRepository = analiseRepository;
    }

    public async Task<AnaliseDto> Handle(AtualizarStatusPorSoatIdCommand command, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        try
        {
            LogInicio(metodo, command);

            var analise = await _analiseRepository.ObterPorSoatAnalysisIdAsync(command.SoatAnalysisId, cancellationToken)
                ?? throw new Exception($"Analise não encontrada para soat_analysis_id: {command.SoatAnalysisId}");

            analise.AtualizarStatus(SafeFromExternalStatus(command.Status));

            _analiseRepository.Atualizar(analise);
            await CommitAsync(cancellationToken);

            var resultado = analise.ToDto();

            LogFim(metodo, resultado);
            return resultado;
        }
        catch (Exception ex)
        {
            LogErro(metodo, ex);
            throw;
        }
    }

    private StatusAnalise SafeFromExternalStatus(string status)
    {
        try
        {
            return StatusAnaliseExtensions.FromExternalStatus(status);
        }
        catch (ArgumentException ex)
        {
            LogErro(nameof(SafeFromExternalStatus), ex);
            return StatusAnalise.Error;
        }
    }
}

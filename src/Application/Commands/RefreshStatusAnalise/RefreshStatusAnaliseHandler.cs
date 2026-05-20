using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Commands.RefreshStatusAnalise;

public class RefreshStatusAnaliseHandler : HandlerBase<RefreshStatusAnaliseHandler>, IRequestHandler<RefreshStatusAnaliseCommand, AnaliseDto?>
{
    private readonly IAnaliseRepository _analiseRepository;
    private readonly IProcessingServiceClient _processingClient;

    public RefreshStatusAnaliseHandler(
        IAnaliseRepository analiseRepository,
        IProcessingServiceClient processingClient,
        IUnitOfWork unitOfWork,
        ILogService<RefreshStatusAnaliseHandler> logService)
        : base(logService, unitOfWork)
    {
        _analiseRepository = analiseRepository;
        _processingClient = processingClient;
    }

    public async Task<AnaliseDto?> Handle(RefreshStatusAnaliseCommand command, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        try
        {
            LogInicio(metodo, command);

            var analise = await _analiseRepository.ObterPorIdAsync(command.AnaliseId, cancellationToken);
            if (analise is null)
                return null;

            if (!analise.SoatAnalysisId.HasValue || analise.SoatAnalysisId.Value == Guid.Empty)
                return analise.ToDto();

            var processingStatus = await _processingClient.GetAnalysisStatusAsync(analise.SoatAnalysisId.Value, cancellationToken);
            if (processingStatus is null)
                return analise.ToDto();

            var novoStatus = StatusAnaliseExtensions.FromExternalStatus(processingStatus.Status);

            if (novoStatus == analise.Status)
            {
                LogFim(metodo, "Status inalterado.");
                return analise.ToDto();
            }

            analise.AtualizarStatus(novoStatus);
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
}

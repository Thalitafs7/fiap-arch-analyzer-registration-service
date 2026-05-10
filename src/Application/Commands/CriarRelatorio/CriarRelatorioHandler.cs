using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Commands.CriarRelatorio;

public class CriarRelatorioHandler : HandlerBase<CriarRelatorioHandler>, IRequestHandler<CriarRelatorioCommand, RelatorioDto>
{
    private readonly IDiagramaRepository _diagramaRepository;
    private readonly IRelatorioRepository _relatorioRepository;
    private readonly IAnaliseRepository _analiseRepository;

    public CriarRelatorioHandler(
        IDiagramaRepository diagramaRepository,
        IRelatorioRepository relatorioRepository,
        IAnaliseRepository analiseRepository,
        IUnitOfWork unitOfWork,
        ILogService<CriarRelatorioHandler> logService)
        : base(logService, unitOfWork)
    {
        _analiseRepository = analiseRepository;
        _diagramaRepository = diagramaRepository;
        _relatorioRepository = relatorioRepository;
    }

    public async Task<RelatorioDto> Handle(CriarRelatorioCommand command, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        LogInicio(metodo, command);

        var analise = await _analiseRepository.ObterPorIdAsync(command.AnalysisId);
        var diagrama = await _diagramaRepository.ObterPorAnaliseAsync(analise.Id);

        if (diagrama == null) { throw new Exception("Diagrama não existe"); }

        var relatorio = new Relatorio(command.Report.ExecutiveSummary, command.
            soat_analysis_id, diagrama.Id, command.Report.ComponentsIdentified, command.Report.ArchitecturalRisks,
            command.Report.Recommendations);

        await _relatorioRepository.AdicionarAsync(relatorio, cancellationToken);

        await AtualizarStatusAnalise(analise, command.Status ?? string.Empty, cancellationToken);

        await CommitAsync(cancellationToken);

        var resultado = relatorio.ToDto();

        LogFim(metodo, resultado);

        return resultado;
    }

    private async Task AtualizarStatusAnalise(Analise? analise, string status, CancellationToken cancellationToken)
    {
        analise!.AtualizarStatus(StatusAnaliseExtensions.FromExternalStatus(status));
        _analiseRepository.Atualizar(analise);
    }

    public static StatusAnalise MapStatus(string? status)
    {
        return StatusAnaliseExtensions.FromExternalStatus(status);
    }
}

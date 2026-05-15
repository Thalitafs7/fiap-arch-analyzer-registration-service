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

        // soat_analysis_id é o ID da analise neste serviço (passado pelo processing-service de volta)
        var analiseId = command.soat_analysis_id != Guid.Empty
            ? command.soat_analysis_id
            : command.AnalysisId;

        var analise = await _analiseRepository.ObterPorIdAsync(analiseId)
            ?? throw new Exception($"Analise não encontrada: {analiseId}");

        var diagrama = await _diagramaRepository.ObterPorAnaliseAsync(analise.Id)
            ?? throw new Exception("Diagrama não existe para a analise.");

        var risks = command.Report?.ArchitecturalRisks?
            .Select(r => r.ToString())
            .ToList() ?? new List<string>();

        var nome = string.IsNullOrWhiteSpace(command.Report?.ExecutiveSummary)
            ? "Relatório gerado"
            : command.Report.ExecutiveSummary;

        // AnalysisId = ID interno do processing-service (armazenado para referência cruzada)
        var relatorio = new Relatorio(
            nome,
            command.AnalysisId,
            diagrama.Id,
            command.Report?.ComponentsIdentified ?? new List<string>(),
            risks,
            command.Report?.Recommendations ?? new List<string>(),
            errorMessage: command.ErrorMessage,
            errorStep: command.ErrorStep,
            errorType: command.ErrorType);

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

    public static StatusAnalise MapStatus(string? status) => StatusAnaliseExtensions.FromExternalStatus(status);
}

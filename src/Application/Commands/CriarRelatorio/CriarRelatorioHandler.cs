using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using System.Data;

namespace Application.Commands.CriarRelatorio;

public class CriarRelatorioHandler : HandlerBase<CriarRelatorioHandler>, IRequestHandler<CriarRelatorioCommand, RelatorioDto>
{
    private readonly IDiagramaRepository _diagramaRepository;
    private readonly IRelatorioRepository _relatorioRepository;
    private readonly IErrorRepository _errorRepository;
    private readonly IAnaliseRepository _analiseRepository;
    private readonly IFileManagerService _fileManagerService;
    private readonly ISQSManagerService _sQSManagerService;

    public CriarRelatorioHandler(
        IDiagramaRepository diagramaRepository,
        IRelatorioRepository relatorioRepository,
        IErrorRepository errorRepository,
    IAnaliseRepository analiseRepository,
        IFileManagerService fileManagerService,
        ISQSManagerService sQSManagerService,

        IUnitOfWork unitOfWork,
        ILogService<CriarRelatorioHandler> logService)
        : base(logService, unitOfWork)
    {
        _analiseRepository = analiseRepository;
        _errorRepository = errorRepository;
        _sQSManagerService = sQSManagerService;
        _diagramaRepository = diagramaRepository;
        _relatorioRepository = relatorioRepository;
        _fileManagerService = fileManagerService;
    }

    public async Task<RelatorioDto> Handle(CriarRelatorioCommand command, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        try
        {
            LogInicio(metodo, command);

            var analise = await _analiseRepository.ObterPorIdAsync(command.AnalysisId);
            var diagrama = await _diagramaRepository.ObterPorAnaliseAsync(analise.Id);            

            if (diagrama == null) { throw new Exception("Diagrama não existe"); }            ;


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
        catch (Exception ex)
        {
            LogErro(metodo, ex);
            await _errorRepository.AdicionarAsync(new Error(
                   Guid.NewGuid(),
                  "CriarRelatorioCommand",
                  ex.GetType().ToString() + " - " +
                  ex.Message +
                  ex.StackTrace +
                  ex.Source
              ), cancellationToken);

            await CommitAsync(cancellationToken);

            throw;
        }
    }

    private async Task AtualizarStatusAnalise(Analise? analise, string status, CancellationToken cancellationToken)
    {
        analise.Status = MapStatus(status);
        analise.DataAtualizacao = DateTime.UtcNow;
        _analiseRepository.Atualizar(analise);
    }

    public static string MapStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return "Erro";

        return status.Trim().ToUpperInvariant() switch
        {
            "RECEIVED" => "Recebido",
            "PROCESSING" => "Em Processamento",
            "ANALYZED" => "Analisado",
            "ERROR" => "Erro",
            _ => "Erro"
        };
    }
}

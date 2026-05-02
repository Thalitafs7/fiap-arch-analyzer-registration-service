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
    private readonly IFileManagerService _fileManagerService;
    private readonly ISQSManagerService _sQSManagerService;

    public CriarRelatorioHandler(
        IDiagramaRepository diagramaRepository,
        IRelatorioRepository relatorioRepository,
        IAnaliseRepository analiseRepository,
        IFileManagerService fileManagerService,
        ISQSManagerService sQSManagerService,

        IUnitOfWork unitOfWork,
        ILogService<CriarRelatorioHandler> logService)
        : base(logService, unitOfWork)
    {
        _analiseRepository = analiseRepository;
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


            var diagrama = await _diagramaRepository.ObterPorIdAsync(command.DiagramaId);
            var analise = await _analiseRepository.ObterPorIdAsync(command.AnaliseId);

            if (diagrama == null) { throw new Exception("Diagrama não existe"); }
            ;


            var relatorio = new Relatorio(
                command.Nome,
                command.URLS3Relatorio, diagrama.Id
                );

            await _relatorioRepository.AdicionarAsync(relatorio, cancellationToken);

            await AtualizarStatusAnalise(analise, relatorio, cancellationToken);

            await CommitAsync(cancellationToken);

            var resultado = relatorio.ToDto();

            LogFim(metodo, resultado);

            return resultado;
        }
        catch (Exception ex)
        {
            LogErro(metodo, ex);
            throw;
        }
    }

    private async Task AtualizarStatusAnalise(Analise? analise, Relatorio relatorio, CancellationToken cancellationToken)
    {
        
        analise.Status = StatusAnaliseEnum.Analisado.ToString();
        _analiseRepository.Atualizar(analise);
    }
}

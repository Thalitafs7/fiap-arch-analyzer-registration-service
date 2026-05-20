using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using Application.Mappings;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
namespace Application.Commands.CriarAnalise;

public class CriarAnaliseHandler : HandlerBase<CriarAnaliseHandler>, IRequestHandler<CriarAnaliseCommand, AnaliseDto>
{
    private readonly IAnaliseRepository _analiseRepository;
    private readonly IFileManagerService _fileManagerService;
    private readonly IRabbitMQDiagramPublisher _rabbitPublisher;
    private readonly IRegistrationSettings _settings;

    public CriarAnaliseHandler(
        IAnaliseRepository analiseRepository,
        IFileManagerService fileManagerService,
        IRabbitMQDiagramPublisher rabbitPublisher,
        IRegistrationSettings settings,
        IUnitOfWork unitOfWork,
        ILogService<CriarAnaliseHandler> logService)
        : base(logService, unitOfWork)
    {
        _analiseRepository = analiseRepository;
        _fileManagerService = fileManagerService;
        _rabbitPublisher = rabbitPublisher;
        _settings = settings;
    }

    public async Task<AnaliseDto> Handle(CriarAnaliseCommand command, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        LogInicio(metodo, command);

        var diagramas = new List<Diagrama>();

        var analise = new Analise(
            command.ClienteId,
            command.Nome,
            StatusAnalise.Recebido,
            diagramas,
            command.Descricao);

        foreach (var item in command.Files)
        {
            string fileKey = $"analises/{analise.Id}/diagramas/{item.FileName}";
            try
            {
                await _fileManagerService.UploadAsync(fileKey, item.Content);
            }
            catch (Exception ex)
            {
                LogErro($"{metodo}.fileUpload", ex);
            }
            CriarDiagrama(diagramas, item, fileKey);
        }

        await _analiseRepository.AdicionarAsync(analise, cancellationToken);
        await CommitAsync(cancellationToken);

        var resultado = analise.ToDto();

        var callbackUrl = $"{_settings.CallbackBaseUrl.TrimEnd('/')}/api/webhooks/report/callback";
        var soatAnalysisId = resultado.Id.ToString();

        // Publica no RabbitMQ — fluxo principal para o processing-service
        foreach (var file in command.Files)
        {
            await _rabbitPublisher.PublishAsync(new DiagramMessage(
                JobId: soatAnalysisId,
                FileName: file.FileName,
                FileB64: Convert.ToBase64String(file.Content),
                ContentType: file.ContentType ?? "application/octet-stream",
                SoatAnalysisId: soatAnalysisId,
                CallbackUrl: callbackUrl
            ));
        }

        LogFim(metodo, resultado);

        return resultado;
    }

    private static void CriarDiagrama(List<Diagrama> diagramas, FileData item, string fileKey)
    {
        diagramas.Add(new Diagrama(fileKey, item.FileName, item.ContentType));
    }
}

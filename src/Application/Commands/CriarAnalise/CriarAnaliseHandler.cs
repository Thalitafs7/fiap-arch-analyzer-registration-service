using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using Application.Mappings;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using System.Text.Json;

namespace Application.Commands.CriarAnalise;

public class CriarAnaliseHandler : HandlerBase<CriarAnaliseHandler>, IRequestHandler<CriarAnaliseCommand, AnaliseDto>
{
    private readonly IAnaliseRepository _analiseRepository;
    private readonly IFileManagerService _fileManagerService;
    private readonly ISQSManagerService _sQSManagerService;

    public CriarAnaliseHandler(
        IAnaliseRepository analiseRepository,
        IFileManagerService fileManagerService,
        ISQSManagerService sQSManagerService,
        IUnitOfWork unitOfWork,
        ILogService<CriarAnaliseHandler> logService)
        : base(logService, unitOfWork)
    {
        _sQSManagerService = sQSManagerService;
        _analiseRepository = analiseRepository;
        _fileManagerService = fileManagerService;
    }

    public async Task<AnaliseDto> Handle(CriarAnaliseCommand command, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        LogInicio(metodo, command);

        var diagramas = new List<Diagrama>() { };

        var analise = new Analise(
            command.ClienteId,
            command.Nome,
            StatusAnalise.Recebido,
            diagramas, command.Descricao
            );

        //Criar o arquivo no S3 e obter a URL
        foreach (var item in command.Files)
        {
            string s3keyPrefix = $"analises/{analise.Id}/diagramas/{item.FileName}";
            string urlS3 = $"{s3keyPrefix}/{item.FileName}";
            await _fileManagerService.UploadAsync(urlS3, item.Content);
            CriarDiagrama(diagramas, item, urlS3);
        }

        await _analiseRepository.AdicionarAsync(analise, cancellationToken);

        await CommitAsync(cancellationToken);

        var resultado = analise.ToDto();

        //enviar para fila SQS para processamento assíncrono
        await _sQSManagerService.Send(JsonSerializer.Serialize(new { resultado.Id, resultado.Diagramas }));

        LogFim(metodo, resultado);

        return resultado;
    }

    private static void CriarDiagrama(List<Diagrama> diagramas, FileData item, string urlS3)
    {
        Diagrama diagrama = new Diagrama(urlS3, item.FileName, item.ContentType);

        diagramas.Add(diagrama);
    }
}

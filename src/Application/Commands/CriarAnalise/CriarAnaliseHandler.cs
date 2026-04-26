using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
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

        try
        {
            LogInicio(metodo, command);


            var diagramas = new List<Diagrama>() { };


            var analise = new Analise(
                command.ClienteId,
                command.Nome,
                StatusAnaliseEnum.Recebido.ToString(),
                diagramas
                );


            //Criar o arquivo no S3 e obter a URL
            foreach (var item in command.Files)
            {
                byte[] fileBytes = GetFileBytes(item);
                string s3keyPrefix = $"analises/{analise.Id}/diagramas/{item.FileName}";
                string urlS3 = $"{s3keyPrefix}/{item.FileName}";
                await _fileManagerService.UploadAsync(urlS3, fileBytes);
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
        catch (Exception ex)
        {
            LogErro(metodo, ex);
            throw;
        }
    }

    private static void CriarDiagrama(List<Diagrama> diagramas, IFormFile item, string urlS3)
    {
        Diagrama diagrama = new Diagrama(urlS3, item.FileName, item.ContentType);

        diagramas.Add(diagrama);
    }

    private byte[] GetFileBytes(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        file.OpenReadStream().CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}

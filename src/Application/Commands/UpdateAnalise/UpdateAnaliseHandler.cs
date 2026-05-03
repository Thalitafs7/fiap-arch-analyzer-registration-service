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

namespace Application.Commands.UpdateAnalise;

public class UpdateAnaliseHandler : HandlerBase<UpdateAnaliseHandler>, IRequestHandler<UpdateAnaliseCommand, AnaliseDto>
{
    private readonly IAnaliseRepository _analiseRepository;
    private readonly IFileManagerService _fileManagerService;
    private readonly ISQSManagerService _sQSManagerService;

    public UpdateAnaliseHandler(
        IAnaliseRepository analiseRepository,
        IFileManagerService fileManagerService,
        ISQSManagerService sQSManagerService,
        IUnitOfWork unitOfWork,
        ILogService<UpdateAnaliseHandler> logService)
        : base(logService, unitOfWork)
    {
        _sQSManagerService = sQSManagerService;
        _analiseRepository = analiseRepository;
        _fileManagerService = fileManagerService;
    }

    public async Task<AnaliseDto> Handle(UpdateAnaliseCommand command, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        try
        {
            LogInicio(metodo, command);

            var analise = _analiseRepository.ObterPorIdAsync(command.Id).Result;
            if (analise == null)
                throw new Exception("Análise não encontrada.");

            AtualizarAnalise(command, analise);

            await _analiseRepository.AdicionarAsync(analise, cancellationToken);


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

    private static void AtualizarAnalise(UpdateAnaliseCommand command, Analise analise)
    {
        analise.Nome = command.Nome;
        analise.Descricao = command.Descricao;
    }  
}

using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Interfaces;
using MediatR;

namespace Application.Commands.DeletarAnalise;

public class DeletarAnaliseHandler : HandlerBase<DeletarAnaliseHandler>, IRequestHandler<DeletarAnaliseCommand, AnaliseDto>
{
    private readonly IAnaliseRepository _analiseRepository;


    public DeletarAnaliseHandler(
        IAnaliseRepository analiseRepository,
        IUnitOfWork unitOfWork,
        ILogService<DeletarAnaliseHandler> logService)
        : base(logService, unitOfWork)
    {
        _analiseRepository = analiseRepository;

    }

    public async Task<AnaliseDto> Handle(DeletarAnaliseCommand command, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        try
        {
            LogInicio(metodo, command);

            var analise = _analiseRepository.ObterPorIdAsync(command.Id).Result;
            if (analise == null)
                throw new Exception("Análise não encontrada.");


            _analiseRepository.Deletar(analise);

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

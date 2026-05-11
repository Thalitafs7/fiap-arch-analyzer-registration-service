using Application.Common.Handlers;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Commands.UpdateAnalise;

public class UpdateAnaliseHandler : HandlerBase<UpdateAnaliseHandler>, IRequestHandler<UpdateAnaliseCommand, AnaliseDto>
{
    private readonly IAnaliseRepository _analiseRepository;
    private readonly IErrorRepository _errorRepository;

    public UpdateAnaliseHandler(
        IAnaliseRepository analiseRepository,
        IErrorRepository errorRepository,
        IUnitOfWork unitOfWork,
        ILogService<UpdateAnaliseHandler> logService)
        : base(logService, unitOfWork)
    {
        _analiseRepository = analiseRepository;
        _errorRepository = errorRepository;
    }

    public async Task<AnaliseDto> Handle(UpdateAnaliseCommand command, CancellationToken cancellationToken)
    {
        const string metodo = nameof(Handle);

        try
        {
            LogInicio(metodo, command);

            var analise = _analiseRepository.ObterPorIdAsync(command.Id).Result;
            if (analise == null)
                throw new Exception("An�lise n�o encontrada.");

            AtualizarAnalise(command, analise);

            _analiseRepository.Atualizar(analise);


            await CommitAsync(cancellationToken);

            var resultado = analise.ToDto();

            LogFim(metodo, resultado);

            return resultado;
        }
        catch (Exception ex)
        {
            LogErro(metodo, ex);

            await _errorRepository.AdicionarAsync(new Error(
                    Guid.NewGuid(),
                   "UpdateAnaliseCommand",
                   ex.GetType().ToString() + " - " +
                   ex.Message +
                   ex.StackTrace +
                   ex.Source
               ), cancellationToken);

            await CommitAsync(cancellationToken);

            throw;
        }
    }

    private static void AtualizarAnalise(UpdateAnaliseCommand command, Analise analise)
    {
        analise.AlterarNome(command.Nome);
        analise.AlterarDescricao(command.Descricao);
    }
}

using Application.Commands.UpdateAnalise;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using UnitTests.Generators;

namespace UnitTests.Application.Commands.UpdateAnalise;

/// <summary>
/// Property-Based Tests for UpdateAnaliseHandler.
/// </summary>
public class UpdateAnaliseHandlerPropertyTests
{
    private IAnaliseRepository CreateAnaliseRepo() => Substitute.For<IAnaliseRepository>();
    private IErrorRepository CreateErrorRepo() => Substitute.For<IErrorRepository>();
    private IUnitOfWork CreateUnitOfWork() => Substitute.For<IUnitOfWork>();
    private ILogService<UpdateAnaliseHandler> CreateLogService() => Substitute.For<ILogService<UpdateAnaliseHandler>>();

    private static UpdateAnaliseHandler CreateSut(
        IAnaliseRepository analiseRepo,
        IErrorRepository errorRepo,
        IUnitOfWork unitOfWork,
        ILogService<UpdateAnaliseHandler> logService) =>
        new(analiseRepo, errorRepo, unitOfWork, logService);

    /// <summary>
    /// Property 1: UpdateAnaliseHandler orchestration preserves command values.
    ///
    /// For any valid UpdateAnaliseCommand and existing Analise, handling the command SHALL invoke
    /// AlterarNome with command.Nome, AlterarDescricao with command.Descricao,
    /// call Atualizar on the repository, and call CommitAsync.
    ///
    /// **Validates: Requirements 2.1, 2.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property UpdateAnalise_ValidCommand_CallsMutationsAndPersists()
    {
        var arb = (from cmd in DomainGenerators.ValidUpdateAnaliseCommand().Generator
                   from analise in DomainGenerators.ValidAnalise().Generator
                   select (cmd, analise)).ToArbitrary();

        return Prop.ForAll(arb, tuple =>
        {
            var (command, analise) = tuple;

            // Arrange
            var analiseRepo = CreateAnaliseRepo();
            var errorRepo = CreateErrorRepo();
            var unitOfWork = CreateUnitOfWork();
            var logService = CreateLogService();

            analiseRepo.ObterPorIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Analise?>(analise));
            unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(1);

            var sut = CreateSut(analiseRepo, errorRepo, unitOfWork, logService);

            // Act
            var result = sut.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Assert — 2.1: AlterarNome and AlterarDescricao applied
            bool nomeUpdated = analise.Nome == command.Nome;
            bool descricaoUpdated = analise.Descricao == (command.Descricao ?? string.Empty);

            // 2.2: Atualizar called and CommitAsync called
            analiseRepo.Received(1).Atualizar(analise);
            unitOfWork.Received().CommitAsync(Arg.Any<CancellationToken>());

            return nomeUpdated && descricaoUpdated;
        });
    }

    /// <summary>
    /// Property 2: UpdateAnaliseHandler throws when Analise not found.
    ///
    /// For any valid UpdateAnaliseCommand where repository returns null,
    /// the handler SHALL throw an Exception.
    ///
    /// **Validates: Requirements 2.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property UpdateAnalise_AnaliseNotFound_ThrowsException()
    {
        return Prop.ForAll(DomainGenerators.ValidUpdateAnaliseCommand(), command =>
        {
            // Arrange
            var analiseRepo = CreateAnaliseRepo();
            var errorRepo = CreateErrorRepo();
            var unitOfWork = CreateUnitOfWork();
            var logService = CreateLogService();

            analiseRepo.ObterPorIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Analise?>(null));
            unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(1);
            errorRepo.AdicionarAsync(Arg.Any<Error>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            var sut = CreateSut(analiseRepo, errorRepo, unitOfWork, logService);

            // Act & Assert — 2.3: exception thrown
            bool threw = false;
            try
            {
                sut.Handle(command, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                threw = true;
            }

            return threw;
        });
    }

    /// <summary>
    /// Property 2: UpdateAnaliseHandler error persistence on failure.
    ///
    /// For any exception thrown during UpdateAnaliseHandler execution (Analise not found),
    /// the handler SHALL persist an Error entity via IErrorRepository.AdicionarAsync
    /// and call CommitAsync before re-throwing the original exception.
    ///
    /// **Validates: Requirements 2.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property UpdateAnalise_OnException_PersistsErrorBeforeRethrow()
    {
        return Prop.ForAll(DomainGenerators.ValidUpdateAnaliseCommand(), command =>
        {
            // Arrange
            var analiseRepo = CreateAnaliseRepo();
            var errorRepo = CreateErrorRepo();
            var unitOfWork = CreateUnitOfWork();
            var logService = CreateLogService();

            // Null → triggers exception path
            analiseRepo.ObterPorIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Analise?>(null));
            unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(1);
            errorRepo.AdicionarAsync(Arg.Any<Error>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            var sut = CreateSut(analiseRepo, errorRepo, unitOfWork, logService);

            // Act — swallow exception, we only care about side effects
            try
            {
                sut.Handle(command, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch { /* expected */ }

            // Assert — 2.4: Error persisted via AdicionarAsync before re-throw
            errorRepo.Received(1).AdicionarAsync(Arg.Any<Error>(), Arg.Any<CancellationToken>());
            unitOfWork.Received().CommitAsync(Arg.Any<CancellationToken>());

            return true;
        });
    }
}

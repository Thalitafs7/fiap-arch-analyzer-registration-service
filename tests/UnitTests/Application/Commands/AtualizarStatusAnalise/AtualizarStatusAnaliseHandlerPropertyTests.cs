using Application.Commands.AtualizarStatusAnalise;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using UnitTests.Generators;

namespace UnitTests.Application.Commands.AtualizarStatusAnalise;

/// <summary>
/// Property-Based Tests for AtualizarStatusAnaliseHandler.
/// </summary>
public class AtualizarStatusAnaliseHandlerPropertyTests
{
    private static AtualizarStatusAnaliseHandler CreateSut(
        IAnaliseRepository analiseRepo,
        IUnitOfWork unitOfWork,
        ILogService<AtualizarStatusAnaliseHandler> logService)
        => new(analiseRepo, unitOfWork, logService);

    /// <summary>
    /// Property 3 (part 1): AtualizarStatusAnaliseHandler transitions to EmProcessamento
    ///
    /// For any valid AtualizarStatusAnaliseCommand with an existing Analise, the handler SHALL
    /// call AtualizarStatus(EmProcessamento) on the entity.
    ///
    /// **Validates: Requirements 3.1**
    /// </summary>
    [Fact]
    public void Handle_ExistingAnalise_SetsStatusEmProcessamento()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidAtualizarStatusCommand(),
            DomainGenerators.ValidAnalise(),
            (AtualizarStatusAnaliseCommand command, Analise analise) =>
            {
                // Arrange
                var analiseRepo = Substitute.For<IAnaliseRepository>();
                var unitOfWork = Substitute.For<IUnitOfWork>();
                var logService = Substitute.For<ILogService<AtualizarStatusAnaliseHandler>>();

                analiseRepo.ObterPorIdAsync(command.Id, Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Analise?>(analise));
                unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(1);

                var sut = CreateSut(analiseRepo, unitOfWork, logService);

                // Act
                sut.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

                // Assert — Requirement 3.1: status transitioned to EmProcessamento
                return analise.Status == StatusAnalise.EmProcessamento;
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// Property 3 (part 2): AtualizarStatusAnaliseHandler calls Atualizar and CommitAsync
    ///
    /// For any valid AtualizarStatusAnaliseCommand with an existing Analise, the handler SHALL
    /// invoke IAnaliseRepository.Atualizar and IUnitOfWork.CommitAsync.
    ///
    /// **Validates: Requirements 3.2**
    /// </summary>
    [Fact]
    public void Handle_ExistingAnalise_CallsAtualizarAndCommitAsync()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidAtualizarStatusCommand(),
            DomainGenerators.ValidAnalise(),
            (AtualizarStatusAnaliseCommand command, Analise analise) =>
            {
                // Arrange
                var analiseRepo = Substitute.For<IAnaliseRepository>();
                var unitOfWork = Substitute.For<IUnitOfWork>();
                var logService = Substitute.For<ILogService<AtualizarStatusAnaliseHandler>>();

                analiseRepo.ObterPorIdAsync(command.Id, Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Analise?>(analise));
                unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(1);

                var sut = CreateSut(analiseRepo, unitOfWork, logService);

                // Act
                sut.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

                // Assert — Requirement 3.2: Atualizar and CommitAsync called
                analiseRepo.Received(1).Atualizar(analise);
                unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());

                return true; // NSubstitute throws if calls not received
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// Property 3 (part 3): AtualizarStatusAnaliseHandler throws when Analise not found
    ///
    /// For any AtualizarStatusAnaliseCommand where the repository returns null,
    /// the handler SHALL throw an Exception.
    ///
    /// **Validates: Requirements 3.3**
    /// </summary>
    [Fact]
    public void Handle_NonExistentAnalise_ThrowsException()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidAtualizarStatusCommand(),
            (AtualizarStatusAnaliseCommand command) =>
            {
                // Arrange
                var analiseRepo = Substitute.For<IAnaliseRepository>();
                var unitOfWork = Substitute.For<IUnitOfWork>();
                var logService = Substitute.For<ILogService<AtualizarStatusAnaliseHandler>>();

                analiseRepo.ObterPorIdAsync(command.Id, Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Analise?>(null));

                var sut = CreateSut(analiseRepo, unitOfWork, logService);

                // Act & Assert — Requirement 3.3: exception thrown when Analise not found
                var threw = false;
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

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// Property 3 (part 4): AtualizarStatusAnaliseHandler returns AnaliseDto with correct field mapping
    ///
    /// For any valid AtualizarStatusAnaliseCommand with an existing Analise, the returned
    /// AnaliseDto SHALL have fields matching the updated entity.
    ///
    /// **Validates: Requirements 3.4**
    /// </summary>
    [Fact]
    public void Handle_ExistingAnalise_ReturnsDtoWithCorrectMapping()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidAtualizarStatusCommand(),
            DomainGenerators.ValidAnalise(),
            (AtualizarStatusAnaliseCommand command, Analise analise) =>
            {
                // Arrange
                var analiseRepo = Substitute.For<IAnaliseRepository>();
                var unitOfWork = Substitute.For<IUnitOfWork>();
                var logService = Substitute.For<ILogService<AtualizarStatusAnaliseHandler>>();

                analiseRepo.ObterPorIdAsync(command.Id, Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Analise?>(analise));
                unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(1);

                var sut = CreateSut(analiseRepo, unitOfWork, logService);

                // Act
                var result = sut.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

                // Assert — Requirement 3.4: DTO fields match updated entity
                if (result is null) return false;

                return result.Id == analise.Id
                    && result.ClienteId == analise.ClienteId
                    && result.Nome == analise.Nome
                    && result.Status == StatusAnalise.EmProcessamento.ToString()
                    && result.Descricao == analise.Descricao
                    && result.Diagramas.Count == analise.Diagramas.Count;
            });

        prop.QuickCheckThrowOnFailure();
    }
}

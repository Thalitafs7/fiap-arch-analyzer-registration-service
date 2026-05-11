using Application.Commands.DeletarAnalise;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using UnitTests.Generators;

namespace UnitTests.ApplicationTests.Commands.DeletarAnalise;

/// <summary>
/// Property-Based Tests for DeletarAnaliseHandler.
/// Feature: registration-service-pbt-cleanup
/// </summary>
public class DeletarAnalisePropertyTests
{
    /// <summary>
    /// Property 11: DeletarAnalise soft-delete
    ///
    /// For any existing Analise, handling DeletarAnaliseCommand SHALL:
    ///   - set Ativo = false
    ///   - update DataAtualizacao to a value >= time before deletion
    ///   - call UnitOfWork.CommitAsync
    ///
    /// **Validates: Requirements 8.1, 8.2, 8.3**
    /// </summary>
    [Fact]
    public void DeletarAnalise_SoftDelete_SetsAtivo_False_UpdatesDataAtualizacao_CallsCommit()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            (Analise analise) =>
            {
                // Arrange
                var analiseRepository = Substitute.For<IAnaliseRepository>();
                var unitOfWork = Substitute.For<IUnitOfWork>();
                var logService = Substitute.For<ILogService<DeletarAnaliseHandler>>();

                unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(1);

                analiseRepository.ObterPorIdAsync(analise.Id, Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Analise?>(analise));

                // Simulate repository Deletar delegating to entity.Desativar()
                analiseRepository.When(r => r.Deletar(Arg.Any<Analise>()))
                    .Do(callInfo => callInfo.Arg<Analise>().Desativar());

                var handler = new DeletarAnaliseHandler(analiseRepository, unitOfWork, logService);
                var command = new DeletarAnaliseCommand(analise.Id);

                var beforeDeletion = DateTime.UtcNow;

                // Act
                handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

                // Assert — Requirement 8.1: Ativo = false
                var ativoFalse = analise.Ativo == false;

                // Assert — Requirement 8.2: DataAtualizacao >= time before deletion
                var dataAtualizada = analise.DataAtualizacao != null
                    && analise.DataAtualizacao >= beforeDeletion;

                // Assert — Requirement 8.3: CommitAsync called
                unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
                var commitChamado = true; // NSubstitute throws if not called

                return ativoFalse && dataAtualizada && commitChamado;
            });

        prop.QuickCheckThrowOnFailure();
    }
}

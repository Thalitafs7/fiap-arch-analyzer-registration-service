using Application.Commands.CriarAnalise;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using UnitTests.Generators;

namespace UnitTests.ApplicationTests.Commands.CriarAnalise;

/// <summary>
/// Property-Based Tests for CriarAnaliseHandler.
/// Feature: registration-service-pbt-cleanup
/// </summary>
public class CriarAnalisePropertyTests
{
    /// <summary>
    /// Property 10: CriarAnalise handler orchestration.
    ///
    /// For any valid CriarAnaliseCommand with N files (N≥1):
    ///   - Analise persisted with correct ClienteId, Nome, Status=Recebido
    ///   - Exactly N Diagramas created and associated
    ///   - N files uploaded to S3
    ///   - One SQS message sent
    ///
    /// **Validates: Requirements 7.1, 7.2, 7.3, 7.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property CriarAnalise_Handler_Orchestration()
    {
        var arb = (from clienteId in Arb.Generate<Guid>().Where(g => g != Guid.Empty)
                   from nome in Arb.Generate<NonEmptyString>().Select(s => s.Get).Where(s => !string.IsNullOrWhiteSpace(s))
                   from fileCount in Gen.Choose(1, 5)
                   select (clienteId, nome, fileCount)).ToArbitrary();

        return Prop.ForAll(arb, tuple =>
        {
            var (clienteId, nome, fileCount) = tuple;

            // Arrange
            var analiseRepository = Substitute.For<IAnaliseRepository>();
            var fileManagerService = Substitute.For<IFileManagerService>();
            var sqsManagerService = Substitute.For<ISQSManagerService>();
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var logService = Substitute.For<ILogService<CriarAnaliseHandler>>();

            unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(1);
            fileManagerService.UploadAsync(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(Task.CompletedTask);
            sqsManagerService.Send(Arg.Any<string>()).Returns(Task.CompletedTask);

            Analise? capturedAnalise = null;
            analiseRepository
                .AdicionarAsync(Arg.Do<Analise>(a => capturedAnalise = a), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            var handler = new CriarAnaliseHandler(
                analiseRepository, fileManagerService,
                sqsManagerService, unitOfWork, logService);

            var files = Enumerable.Range(0, fileCount)
                .Select(i => new FileData(
                    new byte[] { 0x89, 0x50, 0x4E, 0x47 },
                    $"diagram_{i}.png",
                    "image/png"))
                .ToList();

            var command = new CriarAnaliseCommand(
                clienteId, "test desc", nome, "Arquitetura", files, "png");

            // Act
            var result = handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Assert — Property 10 invariants

            // 7.1: Analise persisted with correct ClienteId, Nome, Status=Recebido
            bool persistedCorrectly = capturedAnalise != null
                && capturedAnalise.ClienteId == clienteId
                && capturedAnalise.Nome == nome
                && capturedAnalise.Status == StatusAnalise.Recebido;

            // 7.2: Exactly N Diagramas created and associated
            bool correctDiagramaCount = capturedAnalise != null
                && capturedAnalise.Diagramas.Count == fileCount;

            // 7.3: N files uploaded to S3
            fileManagerService.Received(fileCount).UploadAsync(Arg.Any<string>(), Arg.Any<byte[]>());
            bool s3UploadsCalled = true; // Received() throws if not satisfied

            // 7.4: One SQS message sent
            sqsManagerService.Received(1).Send(Arg.Any<string>());
            bool sqsSent = true; // Received() throws if not satisfied

            return persistedCorrectly && correctDiagramaCount && s3UploadsCalled && sqsSent;
        });
    }
}

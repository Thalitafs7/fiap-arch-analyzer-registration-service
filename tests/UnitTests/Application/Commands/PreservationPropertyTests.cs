using Application.Commands.CriarAnalise;
using Application.Commands.CriarRelatorio;
using Application.Commands.DeletarAnalise;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using Application.Queries.ObterAnaliseAllServico;
using Domain.Entities;
using Domain.Entities.Base;
using Domain.Enums;
using Domain.Interfaces;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;

namespace UnitTests.Preservation;

/// <summary>
/// Preservation Property Tests - Functional Behavior Unchanged.
/// These tests capture the CURRENT behavior of the unfixed code to prevent regressions during refactoring.
/// They MUST PASS on unfixed code (confirms baseline behavior to preserve).
///
/// **Validates: Requirements 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.8, 3.9, 3.10, 3.11, 3.12**
/// </summary>
public class PreservationPropertyTests
{
    #region Property: CriarAnalise persists Analise with correct data and creates Diagrama per file

    /// <summary>
    /// **Validates: Requirements 3.1, 3.2**
    /// For all valid CriarAnaliseCommand inputs → handler persists Analise with correct ClienteId, Nome, Status
    /// and creates Diagrama per file.
    /// </summary>
    [Fact]
    public void CriarAnalise_Persists_Entity_With_Correct_Data_And_Creates_Diagrama_Per_File()
    {
        var prop = Prop.ForAll(
            ValidClienteIdArb(),
            ValidNomeArb(),
            ValidFileCountArb(),
            (Guid clienteId, string nome, int fileCount) =>
            {
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
                analiseRepository.AdicionarAsync(Arg.Do<Analise>(a => capturedAnalise = a), Arg.Any<CancellationToken>())
                    .Returns(Task.CompletedTask);

                var handler = new CriarAnaliseHandler(
                    analiseRepository, fileManagerService,
                    sqsManagerService, unitOfWork, logService);

                // Create FileData list
                var files = new List<FileData>();
                for (int i = 0; i < fileCount; i++)
                {
                    files.Add(new FileData(
                        new byte[] { 0x89, 0x50, 0x4E, 0x47 },
                        $"diagram_{i}.png",
                        "image/png"));
                }

                var command = new CriarAnaliseCommand(
                    clienteId, "test desc", nome, "Arquitetura", files, "png");

                // Act
                var result = handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

                // Assert - Preservation properties
                return capturedAnalise != null
                    && capturedAnalise.ClienteId == clienteId
                    && capturedAnalise.Nome == nome
                    && capturedAnalise.Status == StatusAnalise.Recebido
                    && capturedAnalise.Diagramas.Count == fileCount
                    && result != null
                    && result.ClienteId == clienteId
                    && result.Nome == nome;
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion

    #region Property: DeletarAnalise marks Ativo = false with DataAtualizacao updated

    /// <summary>
    /// **Validates: Requirements 3.6**
    /// For all valid delete commands → result is Ativo = false with DataAtualizacao updated.
    /// </summary>
    [Fact]
    public void DeletarAnalise_Marks_Ativo_False_And_Updates_DataAtualizacao()
    {
        var prop = Prop.ForAll(
            ValidGuidArb(),
            (Guid analiseId) =>
            {
                // Arrange
                var analiseRepository = Substitute.For<IAnaliseRepository>();
                var unitOfWork = Substitute.For<IUnitOfWork>();
                var logService = Substitute.For<ILogService<DeletarAnaliseHandler>>();

                unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(1);

                var existingAnalise = new Analise(
                    Guid.NewGuid(), "Test Analise", StatusAnalise.Recebido,
                    new List<Diagrama>(), "desc");

                analiseRepository.ObterPorIdAsync(analiseId, Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Analise?>(existingAnalise));

                var beforeDelete = DateTime.UtcNow;

                // Simulate current repository Deletar behavior (delegates to entity.Desativar())
                analiseRepository.When(r => r.Deletar(Arg.Any<Analise>()))
                    .Do(callInfo =>
                    {
                        var entity = callInfo.Arg<Analise>();
                        entity.Desativar();
                    });

                var handler = new DeletarAnaliseHandler(
                    analiseRepository, unitOfWork, logService);

                var command = new DeletarAnaliseCommand(analiseId);

                // Act
                var result = handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

                // Assert - Preservation: soft-delete marks Ativo = false with DataAtualizacao updated
                return existingAnalise.Ativo == false
                    && existingAnalise.DataAtualizacao != null
                    && existingAnalise.DataAtualizacao >= beforeDelete;
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion

    #region Property: Webhook callback creates Relatorio and updates Analise.Status

    /// <summary>
    /// **Validates: Requirements 3.7**
    /// For all valid webhook callbacks → Relatorio is created and Analise.Status is updated.
    /// </summary>
    [Fact]
    public void WebhookCallback_Creates_Relatorio_And_Updates_Status()
    {
        var validStatuses = new[] { "RECEIVED", "PROCESSING", "ANALYZED", "ERROR" };

        var tupleArb = (from analysisId in Arb.Generate<Guid>().Where(g => g != Guid.Empty)
                        from soatId in Arb.Generate<Guid>().Where(g => g != Guid.Empty)
                        from status in Gen.Elements(validStatuses)
                        from summary in Arb.Generate<NonEmptyString>().Select(s => s.Get).Where(s => !string.IsNullOrWhiteSpace(s))
                        select (analysisId, soatId, status, summary)).ToArbitrary();

        var prop = Prop.ForAll(
            tupleArb,
            (tuple) =>
            {
                var (analysisId, soatId, status, summary) = tuple;
                // Arrange
                var diagramaRepository = Substitute.For<IDiagramaRepository>();
                var relatorioRepository = Substitute.For<IRelatorioRepository>();
                var analiseRepository = Substitute.For<IAnaliseRepository>();
                var unitOfWork = Substitute.For<IUnitOfWork>();
                var logService = Substitute.For<ILogService<CriarRelatorioHandler>>();

                unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(1);

                var existingAnalise = new Analise(
                    Guid.NewGuid(), "Test", StatusAnalise.Recebido,
                    new List<Diagrama>(), "desc");

                var existingDiagrama = new Diagrama("test.png", "image/png", "s3://bucket/test.png");

                analiseRepository.ObterPorIdAsync(analysisId, Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Analise?>(existingAnalise));
                diagramaRepository.ObterPorAnaliseAsync(existingAnalise.Id, Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Diagrama?>(existingDiagrama));

                Relatorio? capturedRelatorio = null;
                relatorioRepository.AdicionarAsync(Arg.Do<Relatorio>(r => capturedRelatorio = r), Arg.Any<CancellationToken>())
                    .Returns(Task.CompletedTask);

                var handler = new CriarRelatorioHandler(
                    diagramaRepository, relatorioRepository,
                    analiseRepository,
                    unitOfWork, logService);

                var report = new ReportDetail
                {
                    ExecutiveSummary = summary,
                    ComponentsIdentified = new List<string> { "Component1" },
                    ArchitecturalRisks = new List<string> { "Risk1" },
                    Recommendations = new List<string> { "Rec1" }
                };

                var command = new CriarRelatorioCommand(
                    analysisId, soatId, status, report, null, null);

                // Act
                var result = handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

                // Assert - Preservation properties
                var expectedStatus = CriarRelatorioHandler.MapStatus(status);

                return capturedRelatorio != null
                    && capturedRelatorio.Nome == summary
                    && capturedRelatorio.IdDiagrama == existingDiagrama.Id
                    && existingAnalise.Status == expectedStatus
                    && result != null;
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion

    #region Property: Query returns Analises with Diagramas collection populated

    /// <summary>
    /// **Validates: Requirements 3.5**
    /// For all valid query requests → response includes Analise with Diagramas collection populated.
    /// </summary>
    [Fact]
    public void Query_Returns_Analises_With_Diagramas_Populated()
    {
        var prop = Prop.ForAll(
            Gen.Choose(1, 5).ToArbitrary(),
            (int count) =>
            {
                // Arrange
                var analiseRepository = Substitute.For<IAnaliseRepository>();
                var logService = Substitute.For<ILogService<ObterAnaliseAllServicoHandler>>();

                var analises = Enumerable.Range(0, count).Select(i =>
                {
                    var diagramas = new List<Diagrama>
                    {
                        new Diagrama($"diagram_{i}.png", "image/png", $"s3://bucket/diagram_{i}.png")
                    };
                    return new Analise(Guid.NewGuid(), $"Analise_{i}",
                        StatusAnalise.Recebido, diagramas, $"desc_{i}");
                }).ToList();

                analiseRepository.ObterTodosAsync(Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<IEnumerable<Analise>>(analises));

                var handler = new ObterAnaliseAllServicoHandler(analiseRepository, logService);
                var query = new ObterAnaliseAllServicoQuery();

                // Act
                var result = handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();
                var resultList = result.ToList();

                // Assert - Preservation properties
                return resultList.Count == count
                    && resultList.All(a => a != null && a.Diagramas != null && a.Diagramas.Count > 0)
                    && resultList.All(a => a!.ClienteId != Guid.Empty
                        && !string.IsNullOrEmpty(a.Nome)
                        && !string.IsNullOrEmpty(a.Status));
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion

    #region Arbitraries

    private static Arbitrary<Guid> ValidGuidArb()
    {
        return Arb.Generate<Guid>().Where(g => g != Guid.Empty).ToArbitrary();
    }

    private static Arbitrary<Guid> ValidClienteIdArb()
    {
        return Arb.Generate<Guid>().Where(g => g != Guid.Empty).ToArbitrary();
    }

    private static Arbitrary<string> ValidNomeArb()
    {
        return Arb.Generate<NonEmptyString>()
            .Select(s => s.Get)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArbitrary();
    }

    private static Arbitrary<int> ValidFileCountArb()
    {
        return Gen.Choose(1, 5).ToArbitrary();
    }

    #endregion
}

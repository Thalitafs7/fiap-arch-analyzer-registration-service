using Application.Commands.CriarRelatorio;
using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using UnitTests.Generators;

namespace UnitTests.ApplicationTests.Commands.CriarRelatorio;

/// <summary>
/// Property-Based Tests for CriarRelatorioHandler.
/// Feature: registration-service-pbt-cleanup
///
/// **Validates: Requirements 9.1, 9.2, 9.3**
/// </summary>
public class CriarRelatorioPropertyTests
{
    #region Property 12: CriarRelatorio creates report with correct associations

    /// <summary>
    /// **Validates: Requirements 9.1, 9.2, 9.3**
    /// For any valid CriarRelatorioCommand:
    ///   - Relatorio persisted with Nome matching ExecutiveSummary
    ///   - Relatorio.IdDiagrama matches the Diagrama associated with the Analise
    ///   - Analise.Status updated according to the external status mapping
    /// </summary>
    [Fact]
    public void CriarRelatorio_Creates_Report_With_Correct_Associations()
    {
        var validStatuses = new[] { "RECEIVED", "PROCESSING", "ANALYZED", "ERROR" };

        var tupleArb = (from analysisId in Arb.Generate<Guid>().Where(g => g != Guid.Empty)
                        from soatId in Arb.Generate<Guid>().Where(g => g != Guid.Empty)
                        from status in Gen.Elements(validStatuses)
                        from summary in Arb.Generate<NonEmptyString>()
                                           .Select(s => s.Get)
                                           .Where(s => !string.IsNullOrWhiteSpace(s))
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
                    Guid.NewGuid(), "Test Analise", StatusAnalise.Recebido,
                    new List<Diagrama>(), "desc");

                var existingDiagrama = new Diagrama("test.png", "image/png", "s3://bucket/test.png");

                analiseRepository.ObterPorIdAsync(analysisId, Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Analise?>(existingAnalise));
                diagramaRepository.ObterPorAnaliseAsync(existingAnalise.Id, Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Diagrama?>(existingDiagrama));

                Relatorio? capturedRelatorio = null;
                relatorioRepository
                    .AdicionarAsync(Arg.Do<Relatorio>(r => capturedRelatorio = r), Arg.Any<CancellationToken>())
                    .Returns(Task.CompletedTask);

                var handler = new CriarRelatorioHandler(
                    diagramaRepository, relatorioRepository,
                    analiseRepository, unitOfWork, logService);

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

                // Assert — Property 12
                var expectedStatus = CriarRelatorioHandler.MapStatus(status);

                // 9.1: Nome matches ExecutiveSummary
                var nomeCorrect = capturedRelatorio != null && capturedRelatorio.Nome == summary;

                // 9.3: IdDiagrama matches the Diagrama associated with the Analise
                var diagramaCorrect = capturedRelatorio != null && capturedRelatorio.IdDiagrama == existingDiagrama.Id;

                // 9.2: Analise.Status updated according to status mapping
                var statusCorrect = existingAnalise.Status == expectedStatus;

                return nomeCorrect && diagramaCorrect && statusCorrect && result != null;
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion
}

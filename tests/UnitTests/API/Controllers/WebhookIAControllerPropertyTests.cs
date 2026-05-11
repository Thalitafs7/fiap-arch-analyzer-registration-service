using API.Controllers;
using Application.Commands.AtualizarStatusAnalise;
using Application.Commands.CriarRelatorio;
using Application.DTOs;
using Application.Queries.ObterAnaliseServicoPorId;
using FsCheck;
using FsCheck.Xunit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using UnitTests.Generators;
using UnitTests.Helpers;

namespace UnitTests.API.Controllers;

/// <summary>
/// Property-based tests for WebhookIAController.
/// Validates: Requirements 21.1, 21.2, 21.3, 21.4
/// </summary>
public class WebhookIAControllerPropertyTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();

    private WebhookIAController CreateSut() =>
        ControllerTestHelper.CreateController<WebhookIAController>(_mediator);

    // -----------------------------------------------------------------------
    // Requirement 21.1 — Criar → OkResult
    // -----------------------------------------------------------------------

    /// <summary>
    /// **Validates: Requirements 21.1**
    /// For any valid CriarRelatorioRequest, Criar SHALL send CriarRelatorioCommand via IMediator and return OkResult.
    /// </summary>
    [Property]
    public Property Criar_ValidRequest_ReturnsOkResult()
    {
        return Prop.ForAll(
            DomainGenerators.ValidGuid(),
            DomainGenerators.ValidGuid(),
            (analysisId, soatId) =>
            {
                var relatorioDto = new RelatorioDto(
                    Guid.NewGuid(), "Relatorio", soatId, Guid.NewGuid(),
                    new List<string>(), new List<string>(), new List<string>(), null);

                _mediator.Send(Arg.Any<CriarRelatorioCommand>(), Arg.Any<CancellationToken>())
                         .Returns(Task.FromResult(relatorioDto));

                var request = new CriarRelatorioRequest
                {
                    AnalysisId = analysisId,
                    Soat_analysis_id = soatId,
                    Status = "completed",
                    Report = new ReportDetail
                    {
                        ExecutiveSummary = "summary",
                        ComponentsIdentified = new List<string>(),
                        ArchitecturalRisks = new List<string>(),
                        Recommendations = new List<string>()
                    },
                    ErrorMessage = null,
                    CompletedAt = DateTimeOffset.UtcNow
                };

                var sut = CreateSut();
                var result = sut.Criar(request, CancellationToken.None).GetAwaiter().GetResult();

                return (result is OkObjectResult).ToProperty();
            });
    }

    // -----------------------------------------------------------------------
    // Requirement 21.2 — ObterPorId com dados → OkResult
    // -----------------------------------------------------------------------

    /// <summary>
    /// **Validates: Requirements 21.2**
    /// For any valid Guid where IMediator returns AnaliseDto, ObterPorId SHALL return OkResult.
    /// </summary>
    [Property]
    public Property ObterPorId_WithData_ReturnsOkResult()
    {
        return Prop.ForAll(
            DomainGenerators.ValidGuid(),
            hash =>
            {
                var analiseDto = new AnaliseDto(
                    Guid.NewGuid(), hash, "Analise", "Recebido", "desc",
                    new List<DiagramaDto>(), DateTime.UtcNow);

                _mediator.Send(Arg.Any<ObterAnaliseServicoPorIdQuery>(), Arg.Any<CancellationToken>())
                         .Returns(Task.FromResult<AnaliseDto?>(analiseDto));

                var sut = CreateSut();
                var result = sut.ObterPorId(hash, CancellationToken.None).GetAwaiter().GetResult();

                return (result is OkObjectResult).ToProperty();
            });
    }

    // -----------------------------------------------------------------------
    // Requirement 21.3 — ObterPorId sem dados → NotFoundResult
    // -----------------------------------------------------------------------

    /// <summary>
    /// **Validates: Requirements 21.3**
    /// For any valid Guid where IMediator returns null, ObterPorId SHALL return NotFoundResult.
    /// </summary>
    [Property]
    public Property ObterPorId_WithoutData_ReturnsNotFoundResult()
    {
        return Prop.ForAll(
            DomainGenerators.ValidGuid(),
            hash =>
            {
                _mediator.Send(Arg.Any<ObterAnaliseServicoPorIdQuery>(), Arg.Any<CancellationToken>())
                         .Returns(Task.FromResult<AnaliseDto?>(null));

                var sut = CreateSut();
                var result = sut.ObterPorId(hash, CancellationToken.None).GetAwaiter().GetResult();

                return (result is NotFoundObjectResult).ToProperty();
            });
    }

    // -----------------------------------------------------------------------
    // Requirement 21.4 — Update status_processing → OkResult
    // -----------------------------------------------------------------------

    /// <summary>
    /// **Validates: Requirements 21.4**
    /// For any valid Guid, Update SHALL send AtualizarStatusAnaliseCommand via IMediator and return OkResult.
    /// </summary>
    [Property]
    public Property Update_ValidHash_ReturnsOkResult()
    {
        return Prop.ForAll(
            DomainGenerators.ValidGuid(),
            hash =>
            {
                var analiseDto = new AnaliseDto(
                    Guid.NewGuid(), hash, "Analise", "EmProcessamento", "desc",
                    new List<DiagramaDto>(), DateTime.UtcNow);

                _mediator.Send(Arg.Any<AtualizarStatusAnaliseCommand>(), Arg.Any<CancellationToken>())
                         .Returns(Task.FromResult(analiseDto));

                var sut = CreateSut();
                var result = sut.Update(hash, CancellationToken.None).GetAwaiter().GetResult();

                return (result is OkObjectResult).ToProperty();
            });
    }
}

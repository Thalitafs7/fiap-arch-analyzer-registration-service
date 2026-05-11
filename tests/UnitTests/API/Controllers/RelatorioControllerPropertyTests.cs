using Application.DTOs;
using Application.Queries.ObterRelatorioAllServico;
using Application.Queries.ObterRelatorioServico;
using API.Controllers;
using FsCheck;
using FsCheck.Xunit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using UnitTests.Generators;
using UnitTests.Helpers;

namespace UnitTests.API.Controllers;

/// <summary>
/// Property-based tests for RelatorioController.
/// Validates: Requirements 20.1, 20.2, 20.3, 20.4
/// </summary>
public class RelatorioControllerPropertyTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();

    private RelatorioController CreateSut() =>
        ControllerTestHelper.CreateController<RelatorioController>(_mediator);

    // -----------------------------------------------------------------------
    // ObterPorId — com dados → OkResult
    // Validates: Requirements 20.1
    // -----------------------------------------------------------------------

    [Property(DisplayName = "ObterPorId com RelatorioDto retorna OkResult")]
    public Property ObterPorId_ComDados_RetornaOk()
    {
        return Prop.ForAll(
            DomainGenerators.ValidGuid(),
            DomainGenerators.ValidRelatorio(),
            (id, relatorio) =>
            {
                // Arrange
                var dto = new RelatorioDto(
                    relatorio.Id,
                    relatorio.Nome,
                    relatorio.Soat_Analysis_Id,
                    relatorio.IdDiagrama,
                    relatorio.Componentes_Identificado,
                    relatorio.Risco_Arquitetura,
                    relatorio.Recomendacao,
                    null);

                _mediator
                    .Send(Arg.Any<ObterRelatorioServicoQuery>(), Arg.Any<CancellationToken>())
                    .Returns(_ => Task.FromResult(dto));

                var sut = CreateSut();

                // Act
                var result = sut.ObterPorId(id, CancellationToken.None).GetAwaiter().GetResult();

                // Assert
                return result is OkObjectResult;
            });
    }

    // -----------------------------------------------------------------------
    // ObterPorId — sem dados → NotFoundResult
    // Validates: Requirements 20.2
    // -----------------------------------------------------------------------

    [Property(DisplayName = "ObterPorId sem dados retorna NotFoundResult")]
    public Property ObterPorId_SemDados_RetornaNotFound()
    {
        return Prop.ForAll(
            DomainGenerators.ValidGuid(),
            id =>
            {
                // Arrange
                _mediator
                    .Send(Arg.Any<ObterRelatorioServicoQuery>(), Arg.Any<CancellationToken>())
                    .Returns(_ => Task.FromResult<RelatorioDto>(null!));

                var sut = CreateSut();

                // Act
                var result = sut.ObterPorId(id, CancellationToken.None).GetAwaiter().GetResult();

                // Assert
                return result is NotFoundObjectResult;
            });
    }

    // -----------------------------------------------------------------------
    // ObterAll — com dados → OkResult
    // Validates: Requirements 20.3
    // -----------------------------------------------------------------------

    [Property(DisplayName = "ObterAll com lista de RelatorioDto retorna OkResult")]
    public Property ObterAll_ComDados_RetornaOk()
    {
        return Prop.ForAll(
            DomainGenerators.ValidRelatorio(),
            relatorio =>
            {
                // Arrange
                var dto = new RelatorioDto(
                    relatorio.Id,
                    relatorio.Nome,
                    relatorio.Soat_Analysis_Id,
                    relatorio.IdDiagrama,
                    relatorio.Componentes_Identificado,
                    relatorio.Risco_Arquitetura,
                    relatorio.Recomendacao,
                    null);

                IEnumerable<RelatorioDto> lista = new[] { dto };

                _mediator
                    .Send(Arg.Any<ObterRelatorioAllServicoQuery>(), Arg.Any<CancellationToken>())
                    .Returns(_ => Task.FromResult(lista));

                var sut = CreateSut();

                // Act
                var result = sut.ObterAll(CancellationToken.None).GetAwaiter().GetResult();

                // Assert
                return result is OkObjectResult;
            });
    }

    // -----------------------------------------------------------------------
    // ObterAll — sem dados → NotFoundResult
    // Validates: Requirements 20.4
    // -----------------------------------------------------------------------

    [Fact(DisplayName = "ObterAll sem dados retorna NotFoundResult")]
    public async Task ObterAll_SemDados_RetornaNotFound()
    {
        // Arrange
        _mediator
            .Send(Arg.Any<ObterRelatorioAllServicoQuery>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult<IEnumerable<RelatorioDto>>(null!));

        var sut = CreateSut();

        // Act
        var result = await sut.ObterAll(CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}

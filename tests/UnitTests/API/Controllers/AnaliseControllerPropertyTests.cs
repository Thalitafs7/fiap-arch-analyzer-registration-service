using API.Controllers;
using Application.Commands.CriarAnalise;
using Application.Commands.DeletarAnalise;
using Application.Commands.UpdateAnalise;
using Application.Common.Models;
using Application.DTOs;
using Application.Queries.ObterAnaliseAllServico;
using FsCheck;
using FsCheck.Xunit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using UnitTests.Generators;
using UnitTests.Helpers;

namespace UnitTests.API.Controllers;

/// <summary>
/// Property-based tests for AnaliseController.
/// Validates: Requirements 19.1, 19.2, 19.3, 19.4, 19.5
/// </summary>
public class AnaliseControllerPropertyTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();

    private AnaliseController CreateSut() =>
        ControllerTestHelper.CreateController<AnaliseController>(_mediator);

    // -----------------------------------------------------------------------
    // Property 1 — Criar com arquivo válido → OkResult
    // Validates: Requirement 19.1
    // -----------------------------------------------------------------------

    [Property(DisplayName = "Criar_ValidFile_ReturnsOkResult")]
    public Property Criar_ValidFile_ReturnsOkResult()
    {
        return Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            analise =>
            {
                // Arrange
                var mediator = Substitute.For<IMediator>();
                var dto = new AnaliseDto(
                    analise.ClienteId,
                    analise.Id,
                    analise.Nome,
                    analise.Status.ToString(),
                    "desc",
                    new List<DiagramaDto>(),
                    analise.DataCadastro);

                mediator.Send(Arg.Any<CriarAnaliseCommand>(), Arg.Any<CancellationToken>())
                        .Returns(Task.FromResult(dto));

                var controller = ControllerTestHelper.CreateController<AnaliseController>(mediator);

                // Build a minimal IFormFile
                var content = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
                var formFile = BuildFormFile(content, "test.pdf", "application/pdf");

                var request = new CriarAnaliseRequest(
                    Nome: analise.Nome,
                    Tipo: "pdf",
                    Descricao: "desc",
                    File: formFile);

                // Act
                var result = controller.Criar(request, CancellationToken.None).GetAwaiter().GetResult();

                // Assert
                return (result is OkObjectResult).ToProperty();
            });
    }

    // -----------------------------------------------------------------------
    // Property 2 — Update → OkResult
    // Validates: Requirement 19.2
    // -----------------------------------------------------------------------

    [Property(DisplayName = "Update_ValidRequest_ReturnsOkResult")]
    public Property Update_ValidRequest_ReturnsOkResult()
    {
        return Prop.ForAll(
            DomainGenerators.ValidUpdateAnaliseCommand(),
            DomainGenerators.ValidAnalise(),
            (command, analise) =>
            {
                // Arrange
                var mediator = Substitute.For<IMediator>();
                var dto = new AnaliseDto(
                    command.ClienteId,
                    command.Id,
                    command.Nome,
                    analise.Status.ToString(),
                    command.Descricao,
                    new List<DiagramaDto>(),
                    DateTime.UtcNow);

                mediator.Send(Arg.Any<UpdateAnaliseCommand>(), Arg.Any<CancellationToken>())
                        .Returns(Task.FromResult(dto));

                var controller = ControllerTestHelper.CreateController<AnaliseController>(mediator);

                var request = new UpdateAnaliseRequest(
                    AnaliseId: command.Id,
                    ClienteId: command.ClienteId,
                    Nome: command.Nome,
                    Descricao: command.Descricao);

                // Act
                var result = controller.Update(request, CancellationToken.None).GetAwaiter().GetResult();

                // Assert
                return (result is OkObjectResult).ToProperty();
            });
    }

    // -----------------------------------------------------------------------
    // Property 3 — Delete → OkResult
    // Validates: Requirement 19.3
    // -----------------------------------------------------------------------

    [Property(DisplayName = "Delete_ValidGuid_ReturnsOkResult")]
    public Property Delete_ValidGuid_ReturnsOkResult()
    {
        return Prop.ForAll(
            DomainGenerators.ValidGuid(),
            DomainGenerators.ValidAnalise(),
            (hash, analise) =>
            {
                // Arrange
                var mediator = Substitute.For<IMediator>();
                var dto = new AnaliseDto(
                    analise.ClienteId,
                    analise.Id,
                    analise.Nome,
                    analise.Status.ToString(),
                    "desc",
                    new List<DiagramaDto>(),
                    analise.DataCadastro);

                mediator.Send(Arg.Any<DeletarAnaliseCommand>(), Arg.Any<CancellationToken>())
                        .Returns(Task.FromResult(dto));

                var controller = ControllerTestHelper.CreateController<AnaliseController>(mediator);

                // Act
                var result = controller.Delete(hash, CancellationToken.None).GetAwaiter().GetResult();

                // Assert
                return (result is OkObjectResult).ToProperty();
            });
    }

    // -----------------------------------------------------------------------
    // Property 4 — Get retorna NotFound quando IMediator retorna null
    // Validates: Requirement 19.4
    // -----------------------------------------------------------------------

    [Fact(DisplayName = "Get_MediatorReturnsNull_ReturnsNotFound")]
    public async Task Get_MediatorReturnsNull_ReturnsNotFound()
    {
        // Arrange
        _mediator.Send(Arg.Any<ObterAnaliseAllServicoQuery>(), Arg.Any<CancellationToken>())
                 .Returns(Task.FromResult<IEnumerable<AnaliseDto>>(null!));

        var controller = CreateSut();

        // Act
        var result = await controller.Get(CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // -----------------------------------------------------------------------
    // Property 5 — Get retorna Ok com dados quando IMediator retorna lista
    // Validates: Requirement 19.5
    // -----------------------------------------------------------------------

    [Property(DisplayName = "Get_MediatorReturnsData_ReturnsOkResult")]
    public Property Get_MediatorReturnsData_ReturnsOkResult()
    {
        return Prop.ForAll(
            Gen.Choose(1, 5).ToArbitrary(),
            DomainGenerators.ValidAnalise(),
            (count, analise) =>
            {
                // Arrange
                var mediator = Substitute.For<IMediator>();
                var dtos = Enumerable.Range(0, count)
                    .Select(_ => new AnaliseDto(
                        analise.ClienteId,
                        Guid.NewGuid(),
                        analise.Nome,
                        analise.Status.ToString(),
                        "desc",
                        new List<DiagramaDto>(),
                        analise.DataCadastro))
                    .ToList();

                mediator.Send(Arg.Any<ObterAnaliseAllServicoQuery>(), Arg.Any<CancellationToken>())
                        .Returns(Task.FromResult<IEnumerable<AnaliseDto>>(dtos));

                var controller = ControllerTestHelper.CreateController<AnaliseController>(mediator);

                // Act
                var result = controller.Get(CancellationToken.None).GetAwaiter().GetResult();

                // Assert
                return (result is OkObjectResult).ToProperty();
            });
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static IFormFile BuildFormFile(byte[] content, string fileName, string contentType)
    {
        var stream = new MemoryStream(content);
        var formFile = Substitute.For<IFormFile>();
        formFile.FileName.Returns(fileName);
        formFile.ContentType.Returns(contentType);
        formFile.Length.Returns(content.Length);
        formFile.CopyToAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    var target = callInfo.ArgAt<Stream>(0);
                    stream.Position = 0;
                    return stream.CopyToAsync(target);
                });
        return formFile;
    }
}

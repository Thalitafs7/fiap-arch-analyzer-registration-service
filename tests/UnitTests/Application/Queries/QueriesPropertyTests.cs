using Application.Common.Interfaces;
using Application.Queries.ObterAnaliseAllServico;
using Application.Queries.ObterAnaliseServicoPorId;
using Application.Queries.ObterOrdensPorCliente;
using Application.Queries.ObterOrdensPorStatus;
using Application.Queries.ObterRelatorioAllServico;
using Application.Queries.ObterRelatorioServico;
using Application.Queries.ObterTodasOrdens;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using UnitTests.Generators;

namespace UnitTests.Application.Queries;

/// <summary>
/// Property-Based Tests for Query Handlers.
/// Feature: registration-service-pbt-cleanup
/// </summary>
public class QueriesPropertyTests
{
    #region Property 13: Query returns all Analises with Diagramas populated

    /// <summary>
    /// **Validates: Requirements 10.1, 10.2**
    /// For any N Analises in repository (each with at least one Diagrama) →
    /// ObterAnaliseAllServicoHandler returns exactly N results, each with non-empty Diagramas collection.
    /// </summary>
    [Fact]
    public void Property13_ObterAll_Returns_Exactly_N_Results_With_NonEmpty_Diagramas()
    {
        var countArb = Gen.Choose(1, 10).ToArbitrary();

        var prop = Prop.ForAll(
            countArb,
            (int n) =>
            {
                // Arrange
                var analiseRepository = Substitute.For<IAnaliseRepository>();
                var logService = Substitute.For<ILogService<ObterAnaliseAllServicoHandler>>();

                var analises = Enumerable.Range(0, n).Select(i =>
                {
                    var diagramas = new List<Diagrama>
                    {
                        new Diagrama($"diag_{i}.png", "image/png", $"s3://bucket/diag_{i}.png")
                    };
                    return new Analise(Guid.NewGuid(), $"Analise_{i}", StatusAnalise.Recebido, diagramas, $"desc_{i}");
                }).ToList();

                analiseRepository.ObterTodosAsync(Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<IEnumerable<Analise>>(analises));

                var handler = new ObterAnaliseAllServicoHandler(analiseRepository, logService);
                var query = new ObterAnaliseAllServicoQuery();

                // Act
                var result = handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult().ToList();

                // Assert
                return result.Count == n
                    && result.All(dto => dto != null && dto.Diagramas != null && dto.Diagramas.Count > 0);
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion

    #region Property 14: Query by Id returns matching entity

    /// <summary>
    /// **Validates: Requirements 10.3**
    /// For any stored Analise queried by its Id →
    /// ObterAnaliseServicoPorIdHandler returns entity whose Id, ClienteId, Nome, Status match stored entity.
    /// </summary>
    [Fact]
    public void Property14_ObterPorId_Returns_Entity_Matching_Stored()
    {
        var analiseArb =
            (from clienteId in Arb.Generate<Guid>().Where(g => g != Guid.Empty)
             from nome in Arb.Generate<NonEmptyString>().Select(s => s.Get).Where(s => !string.IsNullOrWhiteSpace(s))
             from status in Gen.Elements(
                 StatusAnalise.Recebido,
                 StatusAnalise.EmProcessamento,
                 StatusAnalise.Analisado,
                 StatusAnalise.Error)
             let diagramas = new List<Diagrama>
             {
                 new Diagrama("diag.png", "image/png", "s3://bucket/diag.png")
             }
             select new Analise(clienteId, nome, status, diagramas, "desc"))
            .ToArbitrary();

        var prop = Prop.ForAll(
            analiseArb,
            (Analise stored) =>
            {
                // Arrange
                var analiseRepository = Substitute.For<IAnaliseRepository>();
                var logService = Substitute.For<ILogService<ObterAnaliseServicoPorIdHandler>>();

                analiseRepository.ObterPorOrdemServicoIdAsync(stored.Id, Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Analise>(stored));

                var handler = new ObterAnaliseServicoPorIdHandler(analiseRepository, logService);
                var query = new ObterAnaliseServicoPorIdQuery(stored.Id);

                // Act
                var result = handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();

                // Assert
                return result != null
                    && result.Id == stored.Id
                    && result.ClienteId == stored.ClienteId
                    && result.Nome == stored.Nome
                    && result.Status == stored.Status.ToString();
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion

    #region Property 5.1: ObterOrdensPorClienteHandler returns N DTOs for N Analises

    /// <summary>
    /// **Validates: Requirements 5.1**
    /// For any N Analises in repository → ObterOrdensPorClienteHandler returns exactly N AnaliseDto items.
    /// </summary>
    [Fact]
    public void Property5_1_ObterOrdensPorCliente_Returns_Exactly_N_DTOs()
    {
        var countArb = Gen.Choose(0, 10).ToArbitrary();

        var prop = Prop.ForAll(
            countArb,
            (int n) =>
            {
                var analiseRepository = Substitute.For<IAnaliseRepository>();
                var logService = Substitute.For<ILogService<ObterOrdensPorClienteHandler>>();

                var analises = Enumerable.Range(0, n).Select(i =>
                    new Analise(Guid.NewGuid(), $"Analise_{i}", StatusAnalise.Recebido,
                        new List<Diagrama> { new Diagrama($"d_{i}.png", "image/png", $"s3://b/d_{i}.png") },
                        $"desc_{i}")).ToList();

                analiseRepository.ObterTodosAsync(Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<IEnumerable<Analise>>(analises));

                var handler = new ObterOrdensPorClienteHandler(analiseRepository, logService);
                var query = new ObterOrdensPorClienteQuery(Guid.NewGuid());

                var result = handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult().ToList();

                return result.Count == n;
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion

    #region Property 5.2: ObterOrdensPorStatusHandler returns N DTOs for N Analises

    /// <summary>
    /// **Validates: Requirements 5.2**
    /// For any N Analises in repository → ObterOrdensPorStatusHandler returns exactly N AnaliseDto items.
    /// </summary>
    [Fact]
    public void Property5_2_ObterOrdensPorStatus_Returns_Exactly_N_DTOs()
    {
        var countArb = Gen.Choose(0, 10).ToArbitrary();

        var prop = Prop.ForAll(
            countArb,
            (int n) =>
            {
                var analiseRepository = Substitute.For<IAnaliseRepository>();
                var logService = Substitute.For<ILogService<ObterOrdensPorStatusHandler>>();

                var analises = Enumerable.Range(0, n).Select(i =>
                    new Analise(Guid.NewGuid(), $"Analise_{i}", StatusAnalise.EmProcessamento,
                        new List<Diagrama> { new Diagrama($"d_{i}.png", "image/png", $"s3://b/d_{i}.png") },
                        $"desc_{i}")).ToList();

                analiseRepository.ObterTodosAsync(Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<IEnumerable<Analise>>(analises));

                var handler = new ObterOrdensPorStatusHandler(analiseRepository, logService);
                var statusDto = new AnaliseDto(Guid.NewGuid(), Guid.NewGuid(), "nome", "EmProcessamento",
                    "desc", new List<DiagramaDto>(), DateTime.UtcNow);
                var query = new ObterOrdensPorStatusQuery(statusDto);

                var result = handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult().ToList();

                return result.Count == n;
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion

    #region Property 5.3: ObterTodasOrdensHandler returns N DTOs for N Analises

    /// <summary>
    /// **Validates: Requirements 5.3**
    /// For any N Analises in repository → ObterTodasOrdensHandler returns exactly N AnaliseDto items.
    /// </summary>
    [Fact]
    public void Property5_3_ObterTodasOrdens_Returns_Exactly_N_DTOs()
    {
        var countArb = Gen.Choose(0, 10).ToArbitrary();

        var prop = Prop.ForAll(
            countArb,
            (int n) =>
            {
                var analiseRepository = Substitute.For<IAnaliseRepository>();
                var logService = Substitute.For<ILogService<ObterTodasOrdensHandler>>();

                var analises = Enumerable.Range(0, n).Select(i =>
                    new Analise(Guid.NewGuid(), $"Analise_{i}", StatusAnalise.Analisado,
                        new List<Diagrama> { new Diagrama($"d_{i}.png", "image/png", $"s3://b/d_{i}.png") },
                        $"desc_{i}")).ToList();

                analiseRepository.ObterTodosAsync(Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<IEnumerable<Analise>>(analises));

                var handler = new ObterTodasOrdensHandler(analiseRepository, logService);
                var query = new ObterTodasOrdensQuery();

                var result = handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult().ToList();

                return result.Count == n;
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion

    #region Property 5.4: ObterRelatorioAllServicoHandler returns N DTOs for N Relatorios

    /// <summary>
    /// **Validates: Requirements 5.4**
    /// For any N Relatorios in repository → ObterRelatorioAllServicoHandler returns exactly N RelatorioDto items.
    /// </summary>
    [Fact]
    public void Property5_4_ObterRelatorioAllServico_Returns_Exactly_N_DTOs()
    {
        var countArb = Gen.Choose(0, 10).ToArbitrary();

        var prop = Prop.ForAll(
            countArb,
            (int n) =>
            {
                var relatorioRepository = Substitute.For<IRelatorioRepository>();
                var logService = Substitute.For<ILogService<ObterRelatorioAllServicoHandler>>();

                var relatorios = Enumerable.Range(0, n).Select(i =>
                    new Relatorio($"Relatorio_{i}", Guid.NewGuid(), Guid.NewGuid(),
                        new List<string> { $"comp_{i}" },
                        new List<string> { $"risk_{i}" },
                        new List<string> { $"rec_{i}" })).ToList();

                relatorioRepository.ObterTodosAsync(Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<IEnumerable<Relatorio>>(relatorios));

                var handler = new ObterRelatorioAllServicoHandler(relatorioRepository, logService);
                var query = new ObterRelatorioAllServicoQuery();

                var result = handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult().ToList();

                return result.Count == n;
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion

    #region Property 5.5 & 5.6: ObterRelatorioServicoHandler returns correct DTO or null

    /// <summary>
    /// **Validates: Requirements 5.5**
    /// For any existing Relatorio → ObterRelatorioServicoHandler returns DTO matching stored entity fields.
    /// </summary>
    [Fact]
    public void Property5_5_ObterRelatorioServico_Returns_Matching_DTO()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidRelatorio(),
            (Relatorio stored) =>
            {
                var relatorioRepository = Substitute.For<IRelatorioRepository>();
                var logService = Substitute.For<ILogService<ObterRelatorioServicoHandler>>();

                relatorioRepository.ObterPorIdAsync(stored.Id, Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Relatorio?>(stored));

                var handler = new ObterRelatorioServicoHandler(relatorioRepository, logService);
                var query = new ObterRelatorioServicoQuery(stored.Id);

                var result = handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();

                return result != null
                    && result.Id == stored.Id
                    && result.Nome == stored.Nome
                    && result.Soat_Analysis_Id == stored.Soat_Analysis_Id
                    && result.IdDiagrama == stored.IdDiagrama;
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// **Validates: Requirements 5.6**
    /// For a non-existent Id → ObterRelatorioServicoHandler returns null.
    /// </summary>
    [Fact]
    public void Property5_6_ObterRelatorioServico_Returns_Null_For_NonExistent_Id()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidGuid(),
            (Guid id) =>
            {
                var relatorioRepository = Substitute.For<IRelatorioRepository>();
                var logService = Substitute.For<ILogService<ObterRelatorioServicoHandler>>();

                relatorioRepository.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<Relatorio?>(null));

                var handler = new ObterRelatorioServicoHandler(relatorioRepository, logService);
                var query = new ObterRelatorioServicoQuery(id);

                var result = handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();

                return result == null;
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion
}

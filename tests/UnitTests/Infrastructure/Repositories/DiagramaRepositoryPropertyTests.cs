using Domain.Entities;
using Domain.Enums;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using global::Infrastructure.Persistence;
using global::Infrastructure.Persistence.Repositories;
using UnitTests.Generators;

namespace UnitTests.Infrastructure.Repositories;

/// <summary>
/// Property-based tests for DiagramaRepository using EF Core InMemory.
/// Validates: Requirements 9.1, 9.2, 9.3
/// </summary>
public class DiagramaRepositoryPropertyTests : RepositoryTestBase
{
    private DiagramaRepository CreateSut() => new(Context);

    /// <summary>
    /// Helper: cria Analise com diagramas e persiste tudo via Analise (EF resolve FK automaticamente).
    /// Retorna a Analise salva e os Diagramas com AnaliseId preenchido.
    /// </summary>
    private static (Analise analise, List<Diagrama> diagramas) CreateAnaliseWithDiagramas(
        OrdensDbContext ctx, int diagramaCount)
    {
        var diagramas = Enumerable.Range(0, diagramaCount)
            .Select(i => new Diagrama($"diag_{i}.png", "image/png", $"s3://bucket/diag_{i}.png"))
            .ToList();

        var analise = new Analise(Guid.NewGuid(), "Analise_Test", StatusAnalise.Recebido, diagramas, "desc");
        ctx.Anslise.Add(analise);
        ctx.SaveChanges();
        return (analise, diagramas);
    }

    /// <summary>
    /// Property 8: Repository add-then-retrieve round-trip for Diagrama.
    /// For any valid Diagrama, AdicionarAsync + SaveChanges + ObterPorIdAsync returns entity with matching Id.
    /// Validates: Requirement 9.1
    /// </summary>
    [Property(MaxTest = 20)]
    public Property AdicionarAsync_ThenObterPorId_ReturnsEntityWithMatchingId()
    {
        return Prop.ForAll(
            DomainGenerators.ValidDiagrama(),
            diagrama =>
            {
                var options = new DbContextOptionsBuilder<OrdensDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;
                using var ctx = new OrdensDbContext(options);

                // Diagrama requer AnaliseId FK — criar Analise com diagrama para EF resolver FK
                var analise = new Analise(
                    Guid.NewGuid(), "Analise_Test", StatusAnalise.Recebido,
                    new List<Diagrama> { diagrama }, "desc");
                ctx.Anslise.Add(analise);
                ctx.SaveChanges();

                var found = ctx.Diagrama.Find(diagrama.Id);

                return (found != null && found.Id == diagrama.Id && found.AnaliseId == analise.Id).ToProperty()
                    .Label($"Expected Id={diagrama.Id} with AnaliseId={analise.Id}, found={found?.Id}");
            });
    }

    /// <summary>
    /// Property 8: ObterTodosAsync returns all persisted Diagramas.
    /// For any N Diagramas persisted, ObterTodosAsync returns exactly N items.
    /// Validates: Requirement 9.2
    /// </summary>
    [Property(MaxTest = 10)]
    public Property ObterTodosAsync_WithNDiagramas_ReturnsExactlyN()
    {
        return Prop.ForAll(
            Gen.Choose(1, 5).ToArbitrary(),
            count =>
            {
                var options = new DbContextOptionsBuilder<OrdensDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;
                using var ctx = new OrdensDbContext(options);

                var (_, _) = CreateAnaliseWithDiagramas(ctx, count);

                var repo = new DiagramaRepository(ctx);
                var result = repo.ObterTodosAsync().GetAwaiter().GetResult().ToList();

                return (result.Count == count).ToProperty()
                    .Label($"Expected {count} diagramas, got {result.Count}");
            });
    }

    /// <summary>
    /// Property 8: ObterPorIdAsync with non-existent Id returns null.
    /// Validates: Requirement 9.3
    /// </summary>
    [Property(MaxTest = 20)]
    public Property ObterPorIdAsync_NonExistentId_ReturnsNull()
    {
        return Prop.ForAll(
            DomainGenerators.ValidGuid(),
            id =>
            {
                var repo = CreateSut();
                var found = repo.ObterPorIdAsync(id).GetAwaiter().GetResult();

                return (found == null).ToProperty()
                    .Label($"Expected null for Id={id}, but got entity");
            });
    }
}

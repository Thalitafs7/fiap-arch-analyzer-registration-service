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
/// Property-based tests for AnaliseRepository using EF Core InMemory.
/// Validates: Requirements 8.1, 8.2, 8.3, 8.4, 8.5
/// </summary>
public class AnaliseRepositoryPropertyTests : RepositoryTestBase
{
    private static OrdensDbContext CreateIsolatedContext() =>
        new(new DbContextOptionsBuilder<OrdensDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options);

    /// <summary>
    /// Property 8: Repository add-then-retrieve round-trip.
    /// AdicionarAsync + SaveChanges → ObterPorIdAsync retorna entidade com mesmo Id.
    /// Validates: Requirement 8.1
    /// </summary>
    [Property(MaxTest = 20)]
    public Property AdicionarAsync_ThenObterPorId_ReturnsEntity()
    {
        return Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            analise =>
            {
                using var ctx = CreateIsolatedContext();
                var repo = new AnaliseRepository(ctx);

                repo.AdicionarAsync(analise).GetAwaiter().GetResult();
                ctx.SaveChanges();

                var found = repo.ObterPorIdAsync(analise.Id).GetAwaiter().GetResult();

                return (found != null && found.Id == analise.Id).ToProperty()
                    .Label($"Expected entity with Id={analise.Id}, got {(found == null ? "null" : found.Id)}");
            });
    }

    /// <summary>
    /// Property 8: ObterTodosAsync retorna N entidades com Diagramas incluídos.
    /// Validates: Requirement 8.2
    /// </summary>
    [Property(MaxTest = 10)]
    public Property ObterTodosAsync_WithNAnalises_ReturnsExactlyNWithDiagramas()
    {
        return Prop.ForAll(
            Gen.Choose(1, 5).ToArbitrary(),
            count =>
            {
                using var ctx = CreateIsolatedContext();
                var repo = new AnaliseRepository(ctx);

                var analises = Enumerable.Range(0, count)
                    .Select(i =>
                    {
                        var diagramas = new List<Diagrama>
                        {
                            new Diagrama($"diag_{i}.png", "image/png", $"s3://bucket/diag_{i}.png")
                        };
                        return new Analise(Guid.NewGuid(), $"Analise_{i}", StatusAnalise.Recebido, diagramas, $"desc_{i}");
                    })
                    .ToList();

                foreach (var a in analises)
                    repo.AdicionarAsync(a).GetAwaiter().GetResult();
                ctx.SaveChanges();

                var result = repo.ObterTodosAsync().GetAwaiter().GetResult().ToList();

                var countOk = result.Count == count;
                var diagramasIncluded = result.All(a => a.Diagramas != null);

                return (countOk && diagramasIncluded).ToProperty()
                    .Label($"Expected {count} analises with Diagramas, got {result.Count}");
            });
    }

    /// <summary>
    /// Property 8: Atualizar persiste mudanças após SaveChanges.
    /// Validates: Requirement 8.3
    /// </summary>
    [Property(MaxTest = 20)]
    public Property Atualizar_PersistsMudancas()
    {
        return Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            DomainGenerators.ValidNome(),
            (analise, novoNome) =>
            {
                using var ctx = CreateIsolatedContext();
                var repo = new AnaliseRepository(ctx);

                repo.AdicionarAsync(analise).GetAwaiter().GetResult();
                ctx.SaveChanges();

                analise.AlterarNome(novoNome);
                repo.Atualizar(analise);
                ctx.SaveChanges();

                var updated = repo.ObterPorIdAsync(analise.Id).GetAwaiter().GetResult();

                return (updated != null && updated.Nome == novoNome).ToProperty()
                    .Label($"Expected Nome={novoNome}, got {updated?.Nome}");
            });
    }

    /// <summary>
    /// Property 8: Deletar marca Ativo=false após SaveChanges.
    /// Validates: Requirement 8.4
    /// </summary>
    [Property(MaxTest = 20)]
    public Property Deletar_MarcaAtivoFalse()
    {
        return Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            analise =>
            {
                using var ctx = CreateIsolatedContext();
                var repo = new AnaliseRepository(ctx);

                repo.AdicionarAsync(analise).GetAwaiter().GetResult();
                ctx.SaveChanges();

                repo.Deletar(analise);
                ctx.SaveChanges();

                var found = repo.ObterPorIdAsync(analise.Id).GetAwaiter().GetResult();

                return (found != null && found.Ativo == false).ToProperty()
                    .Label($"Expected Ativo=false, got Ativo={found?.Ativo}");
            });
    }

    /// <summary>
    /// Property 8: ObterPorIdAsync com Id inexistente retorna null.
    /// Validates: Requirement 8.5
    /// </summary>
    [Property(MaxTest = 20)]
    public Property ObterPorIdAsync_WithNonExistentId_ReturnsNull()
    {
        return Prop.ForAll(
            DomainGenerators.ValidGuid(),
            id =>
            {
                using var ctx = CreateIsolatedContext();
                var repo = new AnaliseRepository(ctx);

                var found = repo.ObterPorIdAsync(id).GetAwaiter().GetResult();

                return (found == null).ToProperty()
                    .Label($"Expected null for non-existent Id={id}, got {found?.Id}");
            });
    }
}

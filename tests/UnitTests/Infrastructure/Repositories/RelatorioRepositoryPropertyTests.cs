using Domain.Entities;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using UnitTests.Generators;
using global::Infrastructure.Persistence;
using global::Infrastructure.Persistence.Repositories;

namespace UnitTests.Infrastructure.Repositories;

/// <summary>
/// Property-based tests for RelatorioRepository using EF Core InMemory.
/// Validates: Requirements 11.1, 11.2, 11.3
/// </summary>
public class RelatorioRepositoryPropertyTests
{
    /// <summary>
    /// Creates a fresh OrdensDbContext with InMemory provider and unique DB name.
    /// Overrides OnModelCreating to skip DiagramaConfiguration (incompatible with InMemory).
    /// </summary>
    private static OrdensDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrdensDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new OrdensDbContext(options);
    }

    // Property 8 (partial): Repository add-then-retrieve round-trip
    // Validates: Requirements 11.1
    [Property(MaxTest = 20)]
    public Property AdicionarAsync_ThenObterPorId_ReturnsEntityWithMatchingId()
    {
        return Prop.ForAll(
            DomainGenerators.ValidRelatorio(),
            relatorio =>
            {
                using var ctx = CreateContext();
                var sut = new RelatorioRepository(ctx);

                sut.AdicionarAsync(relatorio).GetAwaiter().GetResult();
                ctx.SaveChanges();

                var found = sut.ObterPorIdAsync(relatorio.Id).GetAwaiter().GetResult();

                return (found != null && found.Id == relatorio.Id).ToProperty();
            });
    }

    // Property 8 (partial): ObterTodosAsync returns exactly N entities
    // Validates: Requirements 11.2
    [Property(MaxTest = 10)]
    public Property ObterTodosAsync_WithNRelatorios_ReturnsExactlyN()
    {
        return Prop.ForAll(
            Gen.Choose(1, 5).ToArbitrary(),
            n =>
            {
                using var ctx = CreateContext();
                var sut = new RelatorioRepository(ctx);

                var relatorios = Enumerable.Range(0, n)
                    .Select(_ => DomainGenerators.ValidRelatorio().Generator.Sample(0, 1).First())
                    .ToList();

                foreach (var r in relatorios)
                    sut.AdicionarAsync(r).GetAwaiter().GetResult();
                ctx.SaveChanges();

                var all = sut.ObterTodosAsync().GetAwaiter().GetResult().ToList();

                return (all.Count == n).ToProperty();
            });
    }

    // Property 8 (partial): ObterPorIdAsync with non-existent Id returns null
    // Validates: Requirements 11.3
    [Property(MaxTest = 20)]
    public Property ObterPorIdAsync_NonExistentId_ReturnsNull()
    {
        return Prop.ForAll(
            DomainGenerators.ValidGuid(),
            id =>
            {
                using var ctx = CreateContext();
                var sut = new RelatorioRepository(ctx);

                var found = sut.ObterPorIdAsync(id).GetAwaiter().GetResult();

                return (found == null).ToProperty();
            });
    }
}

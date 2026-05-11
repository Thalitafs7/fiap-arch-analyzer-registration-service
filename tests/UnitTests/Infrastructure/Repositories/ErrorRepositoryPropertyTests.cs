using Domain.Entities;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using global::Infrastructure.Persistence;
using global::Infrastructure.Persistence.Repositories;

namespace UnitTests.Infrastructure.Repositories;

/// <summary>
/// Property-based tests for ErrorRepository using EF Core InMemory.
/// Validates: Requirements 10.1, 10.2
/// </summary>
public class ErrorRepositoryPropertyTests : RepositoryTestBase
{
    private ErrorRepository CreateSut() => new(Context);

    /// <summary>
    /// Property 8: Repository add-then-retrieve round-trip for Error.
    /// For any valid Error entity, AdicionarAsync + SaveChanges makes entity retrievable.
    /// Validates: Requirement 10.1
    /// </summary>
    [Property(MaxTest = 20)]
    public Property AdicionarAsync_ThenObterTodos_ContainsEntity()
    {
        return Prop.ForAll(
            Arb.Generate<NonEmptyString>().Select(s => s.Get).ToArbitrary(),
            Arb.Generate<NonEmptyString>().Select(s => s.Get).ToArbitrary(),
            (tipo, descricao) =>
            {
                var options = new DbContextOptionsBuilder<OrdensDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;
                using var ctx = new OrdensDbContext(options);
                var repo = new ErrorRepository(ctx);

                var error = new Error(tipo, descricao);
                repo.AdicionarAsync(error).GetAwaiter().GetResult();
                ctx.SaveChanges();

                var all = repo.ObterTodosAsync().GetAwaiter().GetResult().ToList();

                return (all.Count == 1 && all[0].Id == error.Id).ToProperty()
                    .Label($"Expected 1 entity with Id={error.Id}, got {all.Count}");
            });
    }

    /// <summary>
    /// Property 8: ObterTodosAsync returns all persisted Error entities.
    /// For any N errors persisted, ObterTodosAsync returns exactly N items.
    /// Validates: Requirement 10.2
    /// </summary>
    [Property(MaxTest = 10)]
    public Property ObterTodosAsync_WithNErrors_ReturnsExactlyN()
    {
        return Prop.ForAll(
            Gen.Choose(1, 5).ToArbitrary(),
            count =>
            {
                var options = new DbContextOptionsBuilder<OrdensDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;
                using var ctx = new OrdensDbContext(options);
                var repo = new ErrorRepository(ctx);

                var errors = Enumerable.Range(0, count)
                    .Select(i => new Error($"Tipo_{i}", $"Descricao_{i}"))
                    .ToList();

                foreach (var e in errors)
                    repo.AdicionarAsync(e).GetAwaiter().GetResult();
                ctx.SaveChanges();

                var result = repo.ObterTodosAsync().GetAwaiter().GetResult().ToList();

                return (result.Count == count).ToProperty()
                    .Label($"Expected {count} errors, got {result.Count}");
            });
    }
}

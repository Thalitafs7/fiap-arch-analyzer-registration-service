using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Infrastructure.Repositories;

public abstract class RepositoryTestBase : IDisposable
{
    protected readonly OrdensDbContext Context;

    protected RepositoryTestBase()
    {
        var options = new DbContextOptionsBuilder<OrdensDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        Context = new OrdensDbContext(options);
    }

    public void Dispose() => Context.Dispose();
}

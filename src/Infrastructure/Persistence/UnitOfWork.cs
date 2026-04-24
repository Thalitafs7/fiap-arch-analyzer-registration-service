using Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
public class UnitOfWork : IUnitOfWork
{
    private readonly OrdensDbContext _context;

    public UnitOfWork(OrdensDbContext context)
    {
        _context = context;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

using Domain.Entities;
using Domain.Interfaces;

namespace Infrastructure.Persistence.Repositories;

public class ErrorRepository : IErrorRepository
{
    private readonly OrdensDbContext _context;

    public ErrorRepository(OrdensDbContext context)
    {
        _context = context;
    }


    public async Task AdicionarAsync(Error entity, CancellationToken cancellationToken = default)
    {
        await _context.Error.AddAsync(entity, cancellationToken);
    }

    public void Atualizar(Error entity)
    {
        throw new NotImplementedException();
    }

    public void Deletar(Error entity)
    {
        throw new NotImplementedException();
    }

    public Task<Error?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Error>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Remover(Error entity)
    {
        throw new NotImplementedException();
    }
}

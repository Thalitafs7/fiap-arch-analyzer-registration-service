using Domain.Entities;

namespace Domain.Interfaces;

public interface IDiagramaRepository : IRepository<Diagrama>
{
    Task<Diagrama?> ObterComDetalhesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Diagrama>> ObterPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<Diagrama?> ObterPorAnaliseAsync(Guid analiseId, CancellationToken cancellationToken = default);
}

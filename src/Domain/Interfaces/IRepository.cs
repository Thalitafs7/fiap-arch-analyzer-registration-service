using Domain.Entities.Base;

namespace Domain.Interfaces;

public interface IRepository<T> where T : Entity
{
    Task<T?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(T entity, CancellationToken cancellationToken = default);
    void Atualizar(T entity);
    void Remover(T entity);
}

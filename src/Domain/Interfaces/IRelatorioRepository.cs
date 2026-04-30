using Domain.Entities;

namespace Domain.Interfaces;

public interface IRelatorioRepository : IRepository<Relatorio>
{
    Task<Relatorio?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}

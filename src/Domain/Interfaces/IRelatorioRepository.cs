using Domain.Entities;

namespace Domain.Interfaces;

public interface IRelatorioRepository : IRepository<Relatorio>
{
    Task<Relatorio?> ObterPorOrdemServicoIdAsync(Guid ordemServicoId, CancellationToken cancellationToken = default);
}

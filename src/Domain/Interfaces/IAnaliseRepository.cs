using Domain.Entities;

namespace Domain.Interfaces;

public interface IAnaliseRepository : IRepository<Analise>
{
    Task<Analise?> ObterPorOrdemServicoIdAsync(Guid ordemServicoId, CancellationToken cancellationToken = default);

}

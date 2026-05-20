using Domain.Entities;

namespace Domain.Interfaces;

public interface IAnaliseRepository : IRepository<Analise>
{
    Task<Analise> ObterPorOrdemServicoIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Analise?> ObterPorSoatAnalysisIdAsync(Guid soatAnalysisId, CancellationToken cancellationToken = default);
}

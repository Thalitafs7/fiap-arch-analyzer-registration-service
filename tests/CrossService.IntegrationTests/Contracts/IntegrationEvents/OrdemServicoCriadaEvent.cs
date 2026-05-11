namespace CrossService.IntegrationTests.Contracts.IntegrationEvents;

/// <summary>
/// Evento publicado por ms-ordens quando uma OS é criada
/// Consumido por ms-estoque (OrdemServicoCriadaConsumer)
/// </summary>
public record OrdemServicoCriadaEvent
{
    public Guid OrdemServicoId { get; init; }
    public Guid ClienteId { get; init; }
    public IEnumerable<InsumoSolicitado> Insumos { get; init; } = Enumerable.Empty<InsumoSolicitado>();
    public DateTime DataEvento { get; init; } = DateTime.UtcNow;
}

public record InsumoSolicitado
{
    public Guid EstoqueId { get; init; }
    public int Quantidade { get; init; }
}

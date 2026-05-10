namespace CrossService.IntegrationTests.Contracts.IntegrationEvents;

/// <summary>
/// Evento publicado por ms-ordens quando orçamento é cancelado
/// Consumido por ms-estoque (OrcamentoCanceladoConsumer)
/// </summary>
public record OrcamentoCanceladoEvent
{
    public Guid OrdemServicoId { get; init; }
    public DateTime DataEvento { get; init; } = DateTime.UtcNow;
}

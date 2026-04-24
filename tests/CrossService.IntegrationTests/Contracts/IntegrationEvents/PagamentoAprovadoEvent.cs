namespace CrossService.IntegrationTests.Contracts.IntegrationEvents;

/// <summary>
/// Evento publicado por ms-ordens quando pagamento é aprovado
/// Consumido por ms-estoque (PagamentoAprovadoConsumer)
/// </summary>
public record PagamentoAprovadoEvent
{
    public Guid OrdemServicoId { get; init; }
    public Guid PagamentoId { get; init; }
    public decimal Valor { get; init; }
    public DateTime DataEvento { get; init; } = DateTime.UtcNow;
}

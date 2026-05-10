namespace CrossService.IntegrationTests.Contracts.IntegrationEvents;

/// <summary>
/// Evento publicado por ms-estoque quando reserva falha
/// Consumido por ms-ordens (ReservaFalhouConsumer)
/// </summary>
public record ReservaFalhouEvent
{
    public Guid OrdemServicoId { get; init; }
    public string Motivo { get; init; } = string.Empty;
    public DateTime DataEvento { get; init; } = DateTime.UtcNow;
}

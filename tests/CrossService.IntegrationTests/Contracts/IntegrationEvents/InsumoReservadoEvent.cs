namespace CrossService.IntegrationTests.Contracts.IntegrationEvents;

/// <summary>
/// Evento publicado por ms-estoque quando insumos são reservados
/// Consumido por ms-ordens (InsumoReservadoConsumer)
/// </summary>
public record InsumoReservadoEvent
{
    public Guid OrdemServicoId { get; init; }
    public Guid ReservaId { get; init; }
    public DateTime DataEvento { get; init; } = DateTime.UtcNow;
}

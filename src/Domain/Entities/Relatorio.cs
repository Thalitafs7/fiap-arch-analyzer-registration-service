using Domain.Entities.Base;

namespace Domain.Entities;

public class Relatorio : Entity
{
    public string? Nome { get; private set; }
    public string? URLS3Relatorio { get; private set; }

    public Diagrama Diagrama { get; private set; }
    public Guid? IdDiagrama { get; private set; }
}
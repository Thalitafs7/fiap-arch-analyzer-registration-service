using Domain.Entities.Base;

namespace Domain.Entities;
public class Error : Entity
{
    public Guid Id { get; private set; }
    public Guid OrdemServicoId { get; private set; }
    public Guid VeiculoId { get; private set; }
    public Guid? ClienteId { get; private set; }

    public string TipoInspecao { get; private set; } = default!;
    public DateTime DataInspecao { get; private set; }
    public DateTime? DataExpiracao { get; private set; }

    public Guid InspetorId { get; private set; }
    public string InspetorNome { get; private set; } = default!;

    public string? AssinaturaCliente { get; private set; }
    public string? AssinaturaInspetor { get; private set; }
    public List<string> FotosGerais { get; private set; } = new();

    public int Quilometragem { get; private set; }

    public DateTime DataCadastro { get; private set; }
}
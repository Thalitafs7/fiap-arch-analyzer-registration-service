using Domain.Entities.Base;

namespace Domain.Entities;
public class Error : Entity
{
    public Guid Id { get; private set; }
    public string? Tipo { get; set; }
    public string Descricao { get; private set; } = default!;
    public DateTime DataCadastro { get; private set; }
}
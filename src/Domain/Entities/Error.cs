using Domain.Entities.Base;

namespace Domain.Entities;

public class Error : Entity
{
    public string? Tipo { get; private set; }
    public string Descricao { get; private set; } = default!;

    protected Error() { }

    public Error(string tipo, string descricao)
    {
        Tipo = tipo;
        Descricao = descricao;
    }

    /// <summary>
    /// Backward-compatible constructor. The id parameter is ignored since Entity base generates Id automatically.
    /// Will be removed when handlers migrate to ExceptionHandlingBehavior (task 3.5).
    /// </summary>
    public Error(Guid id, string tipo, string descricao)
    {
        Id = id;
        Tipo = tipo;
        Descricao = descricao;
    }
}

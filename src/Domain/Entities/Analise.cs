using Domain.Entities.Base;
using Domain.Exceptions;

namespace Domain.Entities;
public class Analise : Entity
{
    public string Nome { get; private set; }
    public string Status { get; private set; }
    public List<Diagrama> Diagramas { get; private set; } = new();


    private Analise() { }


    public Analise(
        Guid clienteId,
        string nome,
        string status,
        List<Diagrama> diagramas,
        string? descricao = null)
    {
        if (clienteId == Guid.Empty)
            throw new DomainException("ClienteId é obrigatório.");

        if (String.IsNullOrEmpty(nome))
            throw new DomainException("Nome é obrigatório.");

        if (String.IsNullOrEmpty(status))
            throw new DomainException("Status é obrigatório.");

        Nome = nome;
        Status = status;
        Diagramas = diagramas;
    }
}
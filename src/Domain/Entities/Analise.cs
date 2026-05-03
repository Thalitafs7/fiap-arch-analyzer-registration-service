using Domain.Entities.Base;
using Domain.Exceptions;

namespace Domain.Entities;
public class Analise : Entity
{
    public Guid ClienteId { get; set; }
    public string Nome { get; set; } // Altere de private set para set público
    public string Status { get; set; }
    public string Descricao { get; set; }
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

        ClienteId = clienteId;        
        Nome = nome;
        Status = status;
        Diagramas = diagramas;
        Descricao = descricao ?? string.Empty;
    }
}
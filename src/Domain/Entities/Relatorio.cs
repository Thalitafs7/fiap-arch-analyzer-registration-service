using Domain.Entities.Base;
using Domain.Exceptions;

namespace Domain.Entities;

public class Relatorio : Entity
{
    public string? Nome { get; private set; }
    public Guid? Soat_Analysis_Id { get; private set; }
    public Diagrama Diagrama { get; private set; } = default!;
    public Guid? IdDiagrama { get; private set; }
    public List<string>? Componentes_Identificado { get; private set; }
    public List<string>? Risco_Arquitetura { get; private set; }
    public List<string>? Recomendacao { get; private set; }
    public string? Message_Error { get; private set; }

    protected Relatorio() { }

    public Relatorio(
        string nome,
        Guid? soat_Analysis_Id,
        Guid idDiagrama,
        List<string> componentes_Identificado,
        List<string> risco_Arquitetura,
        List<string> recomendacao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do relatório é obrigatório.");

        if (idDiagrama == Guid.Empty)
            throw new DomainException("IdDiagrama é obrigatório.");

        Nome = nome;
        Soat_Analysis_Id = soat_Analysis_Id;
        IdDiagrama = idDiagrama;
        Componentes_Identificado = componentes_Identificado ?? new List<string>();
        Risco_Arquitetura = risco_Arquitetura ?? new List<string>();
        Recomendacao = recomendacao ?? new List<string>();
    }
}

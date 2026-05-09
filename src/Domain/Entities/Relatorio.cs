using Domain.Entities.Base;

namespace Domain.Entities;

public class Relatorio : Entity
{
    public string? Nome { get; private set; }
    public Guid? Soat_Analysis_Id { get; private set; }
    public Diagrama Diagrama { get; private set; }
    public Guid? IdDiagrama { get; private set; }
    public List<string>? Componentes_Identificado { get; private set; }
    public List<string>? Risco_Arquitetura { get; private set; }
    public List<string>? Recomendacao { get; private set; }    
    public string? Message_Error { get; private set; }


    private Relatorio() { }

    public Relatorio(string nome, Guid? soat_Analysis_Id, Guid idDiagrama, List<string> componentes_Identificado, List<string> risco_Arquitetura, List<string> recomendacao)
    {   
        Nome = nome;
        Soat_Analysis_Id = soat_Analysis_Id;
        IdDiagrama = idDiagrama;
        Componentes_Identificado = componentes_Identificado;
        Risco_Arquitetura = risco_Arquitetura;
        Recomendacao = recomendacao;
    }
}
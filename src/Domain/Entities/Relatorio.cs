using Domain.Entities.Base;
using Domain.Exceptions;

namespace Domain.Entities;

public class Relatorio : Entity
{
    public string? Nome { get; private set; }
    public string? URLS3Relatorio { get; private set; }
    public Diagrama Diagrama { get; private set; }
    public Guid? IdDiagrama { get; private set; }



    private Relatorio() { }

    public Relatorio(
        string nome,
        string urlS3Relatorio, Guid idDiagrama)
    {
        if (String.IsNullOrEmpty(nome))
            throw new DomainException("Nome é obrigatório.");

        if (String.IsNullOrEmpty(urlS3Relatorio))
            throw new DomainException("URLS3 relatório é obrigatório.");

        if (idDiagrama == Guid.Empty)
            throw new DomainException("Id do diagrama é obrigatório.");

        Nome = nome;
        URLS3Relatorio = urlS3Relatorio;
        IdDiagrama = idDiagrama;
    }
}
using Domain.Entities.Base;
using Domain.Exceptions;

namespace Domain.Entities;

public class Diagrama : Entity
{
    public string? Nome { get; private set; }
    public string? TipoDiagrama { get; private set; }
    public string? URLS3Diagrama { get; private set; }
    public Relatorio? Relatorio { get; private set; } = default!;
    public Guid IdRelatorio { get; private set; } = default!;
    public Analise Analise { get; private set; } = default!;
    public Guid AnaliseId { get; private set; } = default!;


    private Diagrama() { }

    public Diagrama(
        string nome,
        string tipoDiagrama,
        string urlS3Diagrama)
    {
        if (String.IsNullOrEmpty(nome))
            throw new DomainException("Nome é obrigatório.");

        if (String.IsNullOrEmpty(urlS3Diagrama))
            throw new DomainException("URLS3 diagrama é obrigatório.");

        Nome = nome;
        TipoDiagrama = tipoDiagrama;
        URLS3Diagrama = urlS3Diagrama;
    }
}
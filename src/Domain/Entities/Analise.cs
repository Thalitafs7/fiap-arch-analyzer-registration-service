using Domain.Entities.Base;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class Analise : Entity
{
    public Guid ClienteId { get; private set; }
    public string Nome { get; private set; } = default!;
    public StatusAnalise Status { get; private set; }
    public Guid? SoatAnalysisId { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public List<Diagrama> Diagramas { get; private set; } = new();

    protected Analise() { }

    public Analise(
        Guid clienteId,
        string nome,
        StatusAnalise status,
        List<Diagrama> diagramas,
        string? descricao = null)
    {
        if (clienteId == Guid.Empty)
            throw new DomainException("ClienteId é obrigatório.");

        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome é obrigatório.");

        ClienteId = clienteId;
        Nome = nome;
        Status = status;
        Diagramas = diagramas ?? new List<Diagrama>();
        Descricao = descricao ?? string.Empty;
    }

    public void AlterarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome é obrigatório.");

        Nome = nome;
        AtualizarDataModificacao();
    }

    public void AtualizarStatus(StatusAnalise status)
    {
        Status = status;
        AtualizarDataModificacao();
    }

    public void AtualizarSoatAnalysisId(Guid soatAnalysisId)
    {
        SoatAnalysisId = soatAnalysisId;
        AtualizarDataModificacao();
    }

    public void AlterarDescricao(string descricao)
    {
        if (descricao is null)
            throw new DomainException("Descrição não pode ser nula.");

        Descricao = descricao;
        AtualizarDataModificacao();
    }
}

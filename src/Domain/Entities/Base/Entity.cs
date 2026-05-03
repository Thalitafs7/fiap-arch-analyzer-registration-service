namespace Domain.Entities.Base;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; protected set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }

    protected void AtualizarDataModificacao()
    {
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }

    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}

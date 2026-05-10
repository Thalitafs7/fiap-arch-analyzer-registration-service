using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Builders;

public class EstoqueBuilder
{
    private string _nome = $"Insumo Teste {Guid.NewGuid().ToString("N")[..8]}";
    private string _descricao = "Descrição do insumo de teste";
    private int _quantidadeDisponivel = 100;
    private int _quantidadeMinima = 10;
    private decimal _precoUnitario = 45.00m;

    public static EstoqueBuilder Oleo5W30() => new EstoqueBuilder()
        .ComInsumo($"Óleo 5W30 {Guid.NewGuid().ToString("N")[..8]}")
        .ComDescricao("Óleo lubrificante sintético 5W30")
        .ComQuantidadeDisponivel(50)
        .ComQuantidadeMinima(10)
        .ComPrecoUnitario(45.00m);

    public EstoqueBuilder ComInsumo(string nome) { _nome = nome; return this; }
    public EstoqueBuilder ComDescricao(string descricao) { _descricao = descricao; return this; }
    public EstoqueBuilder ComQuantidadeDisponivel(int quantidade) { _quantidadeDisponivel = quantidade; return this; }
    public EstoqueBuilder ComQuantidadeMinima(int quantidade) { _quantidadeMinima = quantidade; return this; }
    public EstoqueBuilder ComPrecoUnitario(decimal preco) { _precoUnitario = preco; return this; }
    public EstoqueBuilder ComPreco(decimal preco) { _precoUnitario = preco; return this; }

    public CriarEstoqueRequest Build() => new(
        Insumo: _nome,
        Preco: _precoUnitario,
        QuantidadeDisponivel: _quantidadeDisponivel,
        QuantidadeMinima: _quantidadeMinima,
        Descricao: _descricao);
}

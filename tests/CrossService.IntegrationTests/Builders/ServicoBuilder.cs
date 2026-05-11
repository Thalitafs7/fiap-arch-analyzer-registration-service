using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Builders;

public class ServicoBuilder
{
    private string _nome = $"Serviço Teste {Guid.NewGuid().ToString("N")[..8]}";
    private string _descricao = "Descrição do serviço de teste";
    private decimal _valor = 150.00m;
    private bool _disponivel = true;

    public static ServicoBuilder Padrao() => new();

    public ServicoBuilder ComNome(string nome) { _nome = nome; return this; }
    public ServicoBuilder ComDescricao(string descricao) { _descricao = descricao; return this; }
    public ServicoBuilder ComValor(decimal valor) { _valor = valor; return this; }
    public ServicoBuilder Disponivel() { _disponivel = true; return this; }
    public ServicoBuilder Indisponivel() { _disponivel = false; return this; }

    public CriarServicoRequest Build() => new(
        _nome,
        _descricao,
        _valor,
        _disponivel);
}

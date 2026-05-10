using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Builders;

public class OrdemServicoBuilder
{
    private Guid _clienteId = Guid.Empty;
    private Guid _veiculoId = Guid.Empty;
    private Guid _servicoId = Guid.Empty;
    private string _descricao = "Ordem de serviço de teste";
    private List<InsumoRequest> _insumos = new();

    public OrdemServicoBuilder ParaCliente(Guid clienteId) { _clienteId = clienteId; return this; }
    public OrdemServicoBuilder ParaVeiculo(Guid veiculoId) { _veiculoId = veiculoId; return this; }
    public OrdemServicoBuilder ComServico(Guid servicoId) { _servicoId = servicoId; return this; }
    public OrdemServicoBuilder ComDescricao(string descricao) { _descricao = descricao; return this; }

    public OrdemServicoBuilder ComInsumo(Guid estoqueId, int quantidade, decimal precoUnitario)
    {
        _insumos.Add(new InsumoRequest(estoqueId, Insumo: $"Insumo {_insumos.Count + 1}", quantidade, precoUnitario));
        return this;
    }

    public OrdemServicoBuilder ComInsumo(Guid estoqueId, string insumo, int quantidade, decimal precoUnitario)
    {
        _insumos.Add(new InsumoRequest(estoqueId, Insumo: insumo, quantidade, precoUnitario));
        return this;
    }

    public OrdemServicoBuilder SemInsumos()
    {
        _insumos.Clear();
        return this;
    }

    public CriarOrdemServicoRequest Build() => new(
        _clienteId,
        _veiculoId,
        _servicoId,
        _descricao,
        _insumos);
}

public record CriarOrdemServicoRequest(
    Guid ClienteId,
    Guid VeiculoId,
    Guid ServicoId,
    string Descricao,
    List<InsumoRequest> Insumos);

public record InsumoRequest(
    Guid EstoqueId,
    string Insumo,
    int Quantidade,
    decimal PrecoUnitario);

public record IniciarPagamentoRequest(
    Guid OrdemServicoId,
    MetodoPagamento MetodoPagamento,
    string? EmailPagador = null);

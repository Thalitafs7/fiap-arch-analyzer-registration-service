using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Builders;

public class ChecklistBuilder
{
    private Guid _ordemServicoId;
    private Guid _veiculoId;
    private Guid? _clienteId;
    private string _placa = "TST1234";
    private string _marca = "Fiat";
    private string _modelo = "Uno";
    private int _ano = 2020;
    private string? _cor = "Prata";
    private string _tipoInspecao = "completa";
    private Guid _inspetorId = Guid.NewGuid();
    private string _inspetorNome = "Inspetor Teste";
    private int _quilometragem = 50000;
    private int? _diasParaExpirar = 90;
    private readonly List<CategoriaChecklistDto> _categorias = new();

    public ChecklistBuilder ParaOrdemServico(Guid ordemServicoId)
    {
        _ordemServicoId = ordemServicoId;
        return this;
    }

    public ChecklistBuilder ParaVeiculo(Guid veiculoId)
    {
        _veiculoId = veiculoId;
        return this;
    }

    public ChecklistBuilder ParaCliente(Guid clienteId)
    {
        _clienteId = clienteId;
        return this;
    }

    public ChecklistBuilder ComVeiculoSnapshot(string placa, string marca, string modelo, int ano, string? cor)
    {
        _placa = placa;
        _marca = marca;
        _modelo = modelo;
        _ano = ano;
        _cor = cor;
        return this;
    }

    public ChecklistBuilder TipoInspecao(string tipo)
    {
        _tipoInspecao = tipo;
        return this;
    }

    public ChecklistBuilder ComInspetor(Guid inspetorId, string nome)
    {
        _inspetorId = inspetorId;
        _inspetorNome = nome;
        return this;
    }

    public ChecklistBuilder ComQuilometragem(int km)
    {
        _quilometragem = km;
        return this;
    }

    public ChecklistBuilder ComDiasParaExpirar(int? dias)
    {
        _diasParaExpirar = dias;
        return this;
    }

    public ChecklistBuilder ComCategoria(string nome, int ordem,
        (string codigo, string descricao, StatusItemInspecao status)[] itens)
    {
        var categoria = new CategoriaChecklistDto
        {
            Nome = nome,
            Ordem = ordem,
            Itens = itens.Select(i => new ItemChecklistDto
            {
                Codigo = i.codigo,
                Descricao = i.descricao,
                Status = i.status,
                Observacao = null,
                Recomendacao = null,
                Acao = null,
                Fotos = new List<string>(),
                Medicao = null
            }).ToList()
        };
        _categorias.Add(categoria);
        return this;
    }

    public CriarChecklistDto Build()
    {
        return new CriarChecklistDto
        {
            OrdemServicoId = _ordemServicoId,
            VeiculoId = _veiculoId,
            ClienteId = _clienteId,
            Veiculo = new VeiculoSnapshotDto
            {
                Placa = _placa,
                Marca = _marca,
                Modelo = _modelo,
                Ano = _ano,
                Cor = _cor
            },
            TipoInspecao = _tipoInspecao,
            InspetorId = _inspetorId,
            InspetorNome = _inspetorNome,
            Quilometragem = _quilometragem,
            DiasParaExpirar = _diasParaExpirar,
            Categorias = _categorias
        };
    }
}

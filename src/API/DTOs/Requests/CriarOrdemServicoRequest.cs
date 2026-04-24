namespace API.DTOs.Requests;

public record CriarOrdemServicoRequest(
    Guid ClienteId,
    Guid VeiculoId,
    Guid ServicoId,
    string? Descricao = null,
    IEnumerable<InsumoRequest>? Insumos = null
);

public record InsumoRequest(
    Guid EstoqueId,
    string Insumo,
    int Quantidade,
    decimal PrecoUnitario
);

namespace API.DTOs.Requests;

public record GerarOrcamentoRequest(
    Guid OrdemServicoId,
    decimal ValorServico,
    decimal ValorInsumos,
    int DiasValidade = 7
);

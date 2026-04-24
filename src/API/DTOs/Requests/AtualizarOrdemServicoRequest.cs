namespace API.DTOs.Requests;

public record AtualizarOrdemServicoRequest(
    Guid Id,
    string? Descricao = null,
    string? Observacoes = null
);

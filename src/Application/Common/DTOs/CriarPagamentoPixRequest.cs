namespace Application.Common.DTOs;

public record CriarPagamentoPixRequest(
    decimal Valor,
    string Descricao,
    string EmailPagador,
    string? PrimeiroNome = null,
    string? Sobrenome = null,
    string? TipoDocumento = null,
    string? NumeroDocumento = null,
    DateTime? DataExpiracao = null
);

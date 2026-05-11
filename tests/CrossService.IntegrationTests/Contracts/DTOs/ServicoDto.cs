namespace CrossService.IntegrationTests.Contracts.DTOs;

public record ServicoDto(
    Guid Id,
    string Nome,
    string Descricao,
    decimal Valor,
    bool Disponivel,
    DateTime DataCadastro);

public record CriarServicoRequest(
    string Nome,
    string Descricao,
    decimal Valor,
    bool Disponivel = true);

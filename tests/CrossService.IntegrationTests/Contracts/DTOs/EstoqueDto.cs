namespace CrossService.IntegrationTests.Contracts.DTOs;

public record EstoqueDto(
    Guid Id,
    string Nome,
    string Descricao,
    int QuantidadeDisponivel,
    int QuantidadeReservada,
    int QuantidadeMinima,
    decimal PrecoUnitario);

public record CriarEstoqueRequest(
    string Insumo,
    decimal Preco,
    int QuantidadeDisponivel,
    int QuantidadeMinima,
    string? Descricao = null);

public record ReservaDto(
    Guid Id,
    Guid EstoqueId,
    Guid OrdemServicoId,
    int Quantidade,
    string Status,
    DateTime DataExpiracao,
    DateTime DataCadastro);

public record AlertaEstoqueDto(
    Guid Id,
    Guid EstoqueId,
    string Insumo,
    int QuantidadeAtual,
    int QuantidadeMinima,
    bool Notificado,
    DateTime? DataNotificacao,
    DateTime DataCadastro);

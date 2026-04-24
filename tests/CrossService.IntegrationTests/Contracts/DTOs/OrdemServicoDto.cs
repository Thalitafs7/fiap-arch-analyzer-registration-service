namespace CrossService.IntegrationTests.Contracts.DTOs;

public record OrdemServicoDto(
    Guid Id,
    Guid ClienteId,
    Guid VeiculoId,
    Guid ServicoId,
    string? Descricao,
    StatusOrdemServico Status,
    SagaStatus SagaStatus,
    DateTime DataAbertura,
    DateTime? DataFinalizacao,
    OrcamentoDto? Orcamento,
    PagamentoDto? Pagamento,
    IEnumerable<InsumoOSDto> Insumos,
    DateTime DataCadastro);

public record InsumoOSDto(
    Guid Id,
    Guid EstoqueId,
    string Insumo,
    int Quantidade,
    decimal PrecoUnitario,
    decimal Subtotal);

public record OrcamentoDto(
    Guid Id,
    decimal ValorServico,
    decimal ValorInsumos,
    decimal ValorTotal,
    DateTime DataValidade,
    bool? Aceito,
    DateTime? DataResposta,
    bool Expirado);

public record PagamentoDto(
    Guid Id,
    decimal Valor,
    MetodoPagamento MetodoPagamento,
    StatusPagamento Status,
    string? PixQrCode,
    string? PixQrCodeBase64,
    DateTime? PixExpiracao,
    DateTime DataCriacao,
    DateTime? DataAprovacao);

public record GerarOrcamentoRequest(
    Guid OrdemServicoId,
    decimal ValorServico,
    decimal ValorInsumos,
    int ValidadeDias);

public enum StatusOrdemServico
{
    Criada = 0,
    AguardandoAprovacao = 1,
    OrcamentoExpirado = 2,
    Aprovada = 3,
    Recusada = 4,
    AguardandoPagamento = 5,
    Paga = 6,
    EmExecucao = 7,
    Concluida = 8,
    Cancelada = 9
}

public enum SagaStatus
{
    Iniciada = 0,
    Validando = 1,
    OSCriada = 2,
    Reservando = 3,
    Completa = 4,
    Falhou = 5,
    Compensando = 6,
    Compensada = 7
}

public enum MetodoPagamento
{
    Pix = 0,
    CartaoCredito = 1,
    CartaoDebito = 2,
    Boleto = 3
}

public enum StatusPagamento
{
    Pendente = 0,
    Aprovado = 1,
    Recusado = 2,
    Cancelado = 3,
    Expirado = 4
}

public record JobResultDto(int Expirados, string? Message);

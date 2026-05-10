using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Scenarios.Compensacao;

[Collection("IntegrationTests")]
public class TC09_PagamentoRecusadoTests : IntegrationTestBase
{
    public TC09_PagamentoRecusadoTests(TestContainersFixture containers)
        : base(containers) { }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task PagamentoRejeitado_DeveMarcarPagamentoComoRejeitado()
    {
        // Arrange - Criar OS aprovada aguardando pagamento
        var (ordemServico, estoque) = await CriarOSAguardandoPagamentoAsync();

        var estoqueAntes = await EstoqueApi.ObterPorIdAsync(estoque.Id);
        estoqueAntes.Should().NotBeNull();

        // Act - Simular pagamento rejeitado via webhook
        var osAtualizada = await OrdensApi.SimularWebhookPagamentoAsync(ordemServico.Id, status: "rejeitado");

        // Assert - Verificar pagamento rejeitado
        osAtualizada.Should().NotBeNull();
        osAtualizada!.Pagamento.Should().NotBeNull();
        osAtualizada.Pagamento!.Status.Should().Be(StatusPagamento.Recusado);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Medium")]
    public async Task PagamentoAprovado_DeveMarcarOSComoPaga()
    {
        // Arrange
        var (ordemServico, _) = await CriarOSAguardandoPagamentoAsync();

        // Act - Simular pagamento aprovado via webhook
        var osAtualizada = await OrdensApi.SimularWebhookPagamentoAsync(
            ordemServico.Id,
            status: "aprovado");

        // Assert - Verificar pagamento aprovado
        osAtualizada.Should().NotBeNull();
        // Status pode ser Paga ou EmExecucao dependendo do fluxo
        osAtualizada!.Status.Should().BeOneOf(StatusOrdemServico.Paga, StatusOrdemServico.EmExecucao);
        osAtualizada.Pagamento.Should().NotBeNull();
        osAtualizada.Pagamento!.Status.Should().Be(StatusPagamento.Aprovado);
    }

    private async Task<(OrdemServicoDto OS, EstoqueDto Estoque)> CriarOSAguardandoPagamentoAsync()
    {
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente Pagamento Recusado")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço Pagamento")
            .ComValor(500.00m)
            .Disponivel()
            .Build());

        var estoque = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComInsumo("Insumo Pagamento")
            .ComQuantidadeDisponivel(50)
            .ComPreco(100.00m)
            .Build());

        var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .ComInsumo(estoque.Id, "Insumo Pagamento", 2, 100.00m)
            .Build());

        // Aguardar saga completar
        await AsyncHelper.WaitForConditionAsync(
            async () => (await OrdensApi.ObterPorIdAsync(os!.Id))?.SagaStatus == SagaStatus.Completa,
            timeout: TimeSpan.FromSeconds(30));

        // Gerar e aprovar orçamento
        await OrdensApi.GerarOrcamentoAsync(os!.Id);
        await OrdensApi.AprovarOrcamentoAsync(os.Id, true);

        // Iniciar pagamento
        await OrdensApi.IniciarPagamentoAsync(os.Id);

        return (os, estoque);
    }
}

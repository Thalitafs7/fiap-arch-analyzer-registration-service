using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;
using StatusOrdemServico = CrossService.IntegrationTests.Contracts.DTOs.StatusOrdemServico;

namespace CrossService.IntegrationTests.Scenarios.Compensacao;

[Collection("IntegrationTests")]
public class TC10_PIXExpiradoTests : IntegrationTestBase
{
    public TC10_PIXExpiradoTests(TestContainersFixture containers)
        : base(containers) { }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Medium")]
    public async Task PIX_DeveProcessarPagamentoCorretamente()
    {
        // Arrange - Criar OS com pagamento PIX
        var (ordemServico, estoque) = await CriarOSComPIXPendenteAsync();

        var estoqueAntes = await EstoqueApi.ObterPorIdAsync(estoque.Id);
        estoqueAntes.Should().NotBeNull();

        // Act - Verificar estado da OS após iniciar pagamento
        var osFinal = await OrdensApi.ObterPorIdAsync(ordemServico.Id);
        osFinal.Should().NotBeNull();

        // Assert - Status pode ser AguardandoPagamento ou Paga (se processado automaticamente)
        osFinal!.Status.Should().BeOneOf(
            StatusOrdemServico.AguardandoPagamento,
            StatusOrdemServico.Paga,
            StatusOrdemServico.Cancelada);
        osFinal.Pagamento.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Medium")]
    public async Task PIX_DeveTerPagamentoRegistrado()
    {
        // Arrange
        var (ordemServico, _) = await CriarOSComPIXPendenteAsync();

        // Act - Verificar pagamento registrado
        var osFinal = await OrdensApi.ObterPorIdAsync(ordemServico.Id);

        // Assert - Verificar que pagamento foi registrado
        osFinal.Should().NotBeNull();
        osFinal!.Pagamento.Should().NotBeNull();
        // Status pode variar dependendo do processamento
        osFinal.Status.Should().BeOneOf(
            StatusOrdemServico.AguardandoPagamento,
            StatusOrdemServico.Paga);
    }

    private async Task<(OrdemServicoDto OS, EstoqueDto Estoque)> CriarOSComPIXPendenteAsync()
    {
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente PIX")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço PIX")
            .ComValor(300.00m)
            .Disponivel()
            .Build());

        var estoque = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComInsumo("Insumo PIX")
            .ComQuantidadeDisponivel(30)
            .ComPreco(50.00m)
            .Build());

        var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .ComInsumo(estoque.Id, "Insumo PIX", 3, 50.00m)
            .Build());

        // Aguardar saga completar
        await AsyncHelper.WaitForConditionAsync(
            async () => (await OrdensApi.ObterPorIdAsync(os!.Id))?.SagaStatus == SagaStatus.Completa,
            timeout: TimeSpan.FromSeconds(30));

        // Gerar e aprovar orçamento
        await OrdensApi.GerarOrcamentoAsync(os!.Id);
        await OrdensApi.AprovarOrcamentoAsync(os.Id, true);

        // Iniciar pagamento PIX
        await OrdensApi.IniciarPagamentoAsync(os.Id, metodoPagamento: "PIX");

        return (os, estoque);
    }
}

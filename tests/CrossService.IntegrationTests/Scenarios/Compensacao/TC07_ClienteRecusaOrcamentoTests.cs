using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Scenarios.Compensacao;

[Collection("IntegrationTests")]
public class TC07_ClienteRecusaOrcamentoTests : IntegrationTestBase
{
    public TC07_ClienteRecusaOrcamentoTests(TestContainersFixture containers)
        : base(containers) { }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task RecusarOrcamento_DeveLiberarReservasDeEstoque()
    {
        // Arrange - Criar OS com saga completa
        var (ordemServico, estoque, quantidadeReservada) = await CriarOSComSagaCompletaAsync();

        var estoqueAntes = await EstoqueApi.ObterPorIdAsync(estoque.Id);
        estoqueAntes.Should().NotBeNull();

        // Gerar orçamento
        await OrdensApi.GerarOrcamentoAsync(ordemServico.Id);

        // Act - Cliente recusa orçamento
        await OrdensApi.AprovarOrcamentoAsync(ordemServico.Id, false);

        // Assert - Aguardar liberação de reservas
        var reservasLiberadas = await AsyncHelper.WaitForConditionAsync(
            async () =>
            {
                var est = await EstoqueApi.ObterPorIdAsync(estoque.Id);
                return est?.QuantidadeReservada == 0;
            },
            timeout: TimeSpan.FromSeconds(30));

        reservasLiberadas.Should().BeTrue("As reservas deveriam ter sido liberadas");

        // Verificar estados finais
        var osFinal = await OrdensApi.ObterPorIdAsync(ordemServico.Id);
        osFinal.Should().NotBeNull();
        osFinal!.Status.Should().Be(StatusOrdemServico.Cancelada);

        var estoqueFinal = await EstoqueApi.ObterPorIdAsync(estoque.Id);
        estoqueFinal.Should().NotBeNull();
        estoqueFinal!.QuantidadeReservada.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task RecusarOrcamento_DevePublicarOrcamentoCanceladoEvent()
    {
        // Arrange
        var (ordemServico, estoque, _) = await CriarOSComSagaCompletaAsync();

        await OrdensApi.GerarOrcamentoAsync(ordemServico.Id);

        // Act
        await OrdensApi.AprovarOrcamentoAsync(ordemServico.Id, false);

        // Assert - Verificar que evento foi processado (reservas liberadas)
        var eventProcessado = await AsyncHelper.WaitForConditionAsync(
            async () =>
            {
                var os = await OrdensApi.ObterPorIdAsync(ordemServico.Id);
                return os?.Status == StatusOrdemServico.Cancelada;
            },
            timeout: TimeSpan.FromSeconds(30));

        eventProcessado.Should().BeTrue("O evento OrcamentoCancelado deveria ter sido processado");
    }

    private async Task<(OrdemServicoDto OS, EstoqueDto Estoque, int Quantidade)> CriarOSComSagaCompletaAsync()
    {
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente Compensação")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço Compensação")
            .ComValor(150.00m)
            .Disponivel()
            .Build());

        var estoque = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComInsumo("Insumo Compensação")
            .ComQuantidadeDisponivel(100)
            .ComPreco(45.00m)
            .Build());

        var quantidade = 5;
        var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .ComInsumo(estoque.Id, "Insumo Compensação", quantidade, 45.00m)
            .Build());

        // Aguardar saga completar
        await AsyncHelper.WaitForConditionAsync(
            async () => (await OrdensApi.ObterPorIdAsync(os!.Id))?.SagaStatus == SagaStatus.Completa,
            timeout: TimeSpan.FromSeconds(30));

        return (os!, estoque, quantidade);
    }
}

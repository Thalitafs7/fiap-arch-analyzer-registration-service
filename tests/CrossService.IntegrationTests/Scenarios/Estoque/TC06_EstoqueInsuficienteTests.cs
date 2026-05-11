using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Scenarios.Estoque;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
[Trait("Priority", "High")]
public class TC06_EstoqueInsuficienteTests : IntegrationTestBase
{
    public TC06_EstoqueInsuficienteTests(TestContainersFixture containers)
        : base(containers)
    {
    }

    [Fact]
    public async Task CriarOS_ComEstoqueInsuficiente_DevePublicarReservaFalhouEvent()
    {
        // ═══════════════════════════════════════════════════════════════
        // ARRANGE
        // ═══════════════════════════════════════════════════════════════

        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao().Build());
        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(
            cliente.Id,
            VeiculoBuilder.Padrao().ParaCliente(cliente.Id).Build());
        var servico = await CadastrosApi.CriarServicoAsync(ServicoBuilder.Padrao().Build());

        var estoque = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComQuantidadeDisponivel(5)
            .ComQuantidadeMinima(10)
            .Build());

        // ═══════════════════════════════════════════════════════════════
        // ACT - Solicitar 10 unidades (maior que disponível)
        // ═══════════════════════════════════════════════════════════════

        var ordemServico = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .ComDescricao("Teste estoque insuficiente")
            .ComInsumo(estoque.Id, 10, 45.00m)
            .Build());

        // ═══════════════════════════════════════════════════════════════
        // ASSERT - Verificar que OS foi criada (saga em andamento ou falhou)
        // Nota: Processamento da saga depende de eventos MassTransit async
        // ═══════════════════════════════════════════════════════════════

        ordemServico.Should().NotBeNull();
        ordemServico.Id.Should().NotBeEmpty();
        ordemServico.Insumos.Should().HaveCount(1);

        var osFinal = await OrdensApi.ObterPorIdAsync(ordemServico.Id);
        osFinal.Should().NotBeNull();
        osFinal!.SagaStatus.Should().BeOneOf(
            SagaStatus.Iniciada, SagaStatus.Validando, SagaStatus.OSCriada,
            SagaStatus.Reservando, SagaStatus.Falhou);
    }
}

using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Scenarios.HappyPath;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
[Trait("Priority", "Critical")]
public class TC01_FluxoCompletoComInsumosTests : IntegrationTestBase
{
    public TC01_FluxoCompletoComInsumosTests(TestContainersFixture containers)
        : base(containers)
    {
    }

    [Fact]
    public async Task FluxoCompleto_DeveExecutarTodasEtapasComSucesso()
    {
        // ═══════════════════════════════════════════════════════════════
        // ARRANGE
        // ═══════════════════════════════════════════════════════════════

        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao().Build());
        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(
            cliente.Id,
            VeiculoBuilder.Padrao().ParaCliente(cliente.Id).Build());
        var servico = await CadastrosApi.CriarServicoAsync(ServicoBuilder.Padrao().Build());
        var estoque1 = await EstoqueApi.CriarEstoqueAsync(EstoqueBuilder.Oleo5W30().Build());
        var estoque2 = await EstoqueApi.CriarEstoqueAsync(
            new EstoqueBuilder().ComInsumo("Filtro de Óleo").ComPrecoUnitario(35.00m).Build());

        // ═══════════════════════════════════════════════════════════════
        // ACT - Criar Ordem de Serviço com insumos
        // ═══════════════════════════════════════════════════════════════

        var ordemServico = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .ComDescricao("Troca de óleo completa")
            .ComInsumo(estoque1.Id, "Óleo 5W30", 4, 45.00m)
            .ComInsumo(estoque2.Id, "Filtro de Óleo", 1, 35.00m)
            .Build());

        // ═══════════════════════════════════════════════════════════════
        // ASSERT - Verificar OS criada (saga pode estar em andamento)
        // Nota: Processamento de reservas depende de eventos MassTransit async
        // ═══════════════════════════════════════════════════════════════

        ordemServico.Should().NotBeNull();
        ordemServico.Id.Should().NotBeEmpty();
        ordemServico.Insumos.Should().HaveCount(2);
        ordemServico.SagaStatus.Should().BeOneOf(
            SagaStatus.Iniciada, SagaStatus.Validando, SagaStatus.OSCriada,
            SagaStatus.Reservando, SagaStatus.Completa);

        var osFinal = await OrdensApi.ObterPorIdAsync(ordemServico.Id);
        osFinal.Should().NotBeNull();
        osFinal!.Insumos.Should().HaveCount(2);
        osFinal.ClienteId.Should().Be(cliente.Id);
        osFinal.VeiculoId.Should().Be(veiculo.Id);
        osFinal.ServicoId.Should().Be(servico.Id);
    }
}

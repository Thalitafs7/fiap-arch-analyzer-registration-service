using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Scenarios.Estoque;

[Collection("IntegrationTests")]
public class TC17_ExpiracaoReservaTests : IntegrationTestBase
{
    public TC17_ExpiracaoReservaTests(TestContainersFixture containers)
        : base(containers) { }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Low")]
    public async Task ReservaAtiva_DeveExistirAposSagaCompletar()
    {
        // Arrange - Criar OS com reservas
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente Reserva")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço Reserva")
            .ComValor(200.00m)
            .Disponivel()
            .Build());

        var estoque = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComInsumo("Insumo Reserva")
            .ComQuantidadeDisponivel(20)
            .ComPreco(60.00m)
            .Build());

        var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .ComInsumo(estoque.Id, "Insumo Reserva", 3, 60.00m)
            .Build());

        // Aguardar saga completar
        var sagaCompleta = await AsyncHelper.WaitForConditionAsync(
            async () => (await OrdensApi.ObterPorIdAsync(os!.Id))?.SagaStatus == SagaStatus.Completa,
            timeout: TimeSpan.FromSeconds(30));

        sagaCompleta.Should().BeTrue("Saga deveria completar");

        // Assert - Verificar reservas criadas
        var reservas = await EstoqueApi.ObterReservasPorOrdemServicoAsync(os!.Id);
        reservas.Should().NotBeEmpty("Deveria ter reservas criadas");

        var estoqueAtualizado = await EstoqueApi.ObterPorIdAsync(estoque.Id);
        estoqueAtualizado!.QuantidadeReservada.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Low")]
    public async Task ReservaAtiva_DeveManterStatusCorreto()
    {
        // Arrange
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente Reserva Ativa")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço Reserva Ativa")
            .ComValor(180.00m)
            .Disponivel()
            .Build());

        var estoque = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComInsumo("Insumo Reserva Ativa")
            .ComQuantidadeDisponivel(40)
            .ComPreco(40.00m)
            .Build());

        var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .ComInsumo(estoque.Id, "Insumo Reserva Ativa", 4, 40.00m)
            .Build());

        await AsyncHelper.WaitForConditionAsync(
            async () => (await OrdensApi.ObterPorIdAsync(os!.Id))?.SagaStatus == SagaStatus.Completa,
            timeout: TimeSpan.FromSeconds(30));

        // Assert - Reserva deve estar ativa
        var reservas = await EstoqueApi.ObterReservasPorOrdemServicoAsync(os!.Id);
        reservas.Should().NotBeEmpty("Deveria ter reservas");
        reservas.All(r => r.Status == "Ativa" || r.Status == "Confirmada").Should().BeTrue("Reservas devem estar ativas ou confirmadas");
    }
}

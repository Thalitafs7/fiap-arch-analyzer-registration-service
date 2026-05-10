using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Scenarios.Estoque;

[Collection("IntegrationTests")]
public class TC16_ReservaMultiplosInsumosTests : IntegrationTestBase
{
    public TC16_ReservaMultiplosInsumosTests(TestContainersFixture containers)
        : base(containers) { }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Medium")]
    public async Task CriarOS_ComMultiplosInsumos_DeveReservarTodos()
    {
        // Arrange
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente Múltiplos Insumos")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Revisão Completa")
            .ComValor(500.00m)
            .Disponivel()
            .Build());

        // Criar 3 estoques diferentes
        var estoque1 = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComInsumo("Óleo 5W30")
            .ComQuantidadeDisponivel(50)
            .ComPreco(45.00m)
            .Build());

        var estoque2 = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComInsumo("Filtro de Óleo")
            .ComQuantidadeDisponivel(30)
            .ComPreco(35.00m)
            .Build());

        var estoque3 = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComInsumo("Filtro de Ar")
            .ComQuantidadeDisponivel(25)
            .ComPreco(55.00m)
            .Build());

        // Act
        var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .ComInsumo(estoque1.Id, "Óleo 5W30", 5, 45.00m)
            .ComInsumo(estoque2.Id, "Filtro de Óleo", 1, 35.00m)
            .ComInsumo(estoque3.Id, "Filtro de Ar", 1, 55.00m)
            .Build());

        // Assert - Aguardar saga completar
        var sagaCompleta = await AsyncHelper.WaitForConditionAsync(
            async () => (await OrdensApi.ObterPorIdAsync(os!.Id))?.SagaStatus == SagaStatus.Completa,
            timeout: TimeSpan.FromSeconds(30));

        sagaCompleta.Should().BeTrue("Saga deveria completar com múltiplos insumos");

        // Verificar reservas criadas
        var reservas = await EstoqueApi.ObterReservasPorOrdemServicoAsync(os!.Id);
        reservas.Should().HaveCount(3, "Deveria ter 3 reservas para 3 insumos");

        // Verificar cada estoque
        var estoque1Atualizado = await EstoqueApi.ObterPorIdAsync(estoque1.Id);
        estoque1Atualizado!.QuantidadeReservada.Should().Be(5);

        var estoque2Atualizado = await EstoqueApi.ObterPorIdAsync(estoque2.Id);
        estoque2Atualizado!.QuantidadeReservada.Should().Be(1);

        var estoque3Atualizado = await EstoqueApi.ObterPorIdAsync(estoque3.Id);
        estoque3Atualizado!.QuantidadeReservada.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Medium")]
    public async Task CriarOS_ComUmInsumoInsuficiente_DeveFalharTodaReserva()
    {
        // Arrange
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente Falha Parcial")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço Falha Parcial")
            .ComValor(400.00m)
            .Disponivel()
            .Build());

        var estoqueOk = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComInsumo("Insumo OK")
            .ComQuantidadeDisponivel(100)
            .ComPreco(30.00m)
            .Build());

        var estoqueInsuficiente = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComInsumo("Insumo Insuficiente")
            .ComQuantidadeDisponivel(2)  // Só tem 2
            .ComPreco(80.00m)
            .Build());

        // Act - Solicitar 10 do insumo que só tem 2
        var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .ComInsumo(estoqueOk.Id, "Insumo OK", 5, 30.00m)
            .ComInsumo(estoqueInsuficiente.Id, "Insumo Insuficiente", 10, 80.00m)
            .Build());

        // Assert - Saga deve falhar
        var sagaFalhou = await AsyncHelper.WaitForConditionAsync(
            async () => (await OrdensApi.ObterPorIdAsync(os!.Id))?.SagaStatus == SagaStatus.Falhou,
            timeout: TimeSpan.FromSeconds(30));

        sagaFalhou.Should().BeTrue("Saga deveria falhar quando um insumo é insuficiente");

        // Verificar que nenhuma reserva foi mantida
        var estoqueOkAtualizado = await EstoqueApi.ObterPorIdAsync(estoqueOk.Id);
        estoqueOkAtualizado!.QuantidadeReservada.Should().Be(0, "Reservas deveriam ter sido compensadas");
    }
}

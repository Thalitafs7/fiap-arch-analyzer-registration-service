using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Scenarios.Saga;

[Collection("IntegrationTests")]
public class TC11_SagaTimeoutTests : IntegrationTestBase
{
    public TC11_SagaTimeoutTests(TestContainersFixture containers)
        : base(containers) { }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Medium")]
    public async Task SagaCompleta_DeveProcessarReservasCorretamente()
    {
        // Arrange
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente Saga")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço Saga")
            .ComValor(250.00m)
            .Disponivel()
            .Build());

        var estoque = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComInsumo("Insumo Saga")
            .ComQuantidadeDisponivel(50)
            .ComPreco(75.00m)
            .Build());

        // Act - Criar OS com insumos
        var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .ComInsumo(estoque.Id, "Insumo Saga", 2, 75.00m)
            .Build());

        os.Should().NotBeNull();

        // Assert - Verificar se saga completa normalmente
        var sagaCompleta = await AsyncHelper.WaitForConditionAsync(
            async () =>
            {
                var o = await OrdensApi.ObterPorIdAsync(os!.Id);
                return o?.SagaStatus == SagaStatus.Completa;
            },
            timeout: TimeSpan.FromSeconds(30));

        sagaCompleta.Should().BeTrue("Saga deveria completar com sucesso");

        // Verificar reservas criadas
        var reservas = await EstoqueApi.ObterReservasPorOrdemServicoAsync(os!.Id);
        reservas.Should().NotBeEmpty("Deveria ter reservas criadas");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Medium")]
    public async Task SagaFalha_DeveCompensarReservasQuandoEstoqueInsuficiente()
    {
        // Arrange - Criar estoque com quantidade insuficiente
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente Saga Falha")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço Saga Falha")
            .ComValor(180.00m)
            .Disponivel()
            .Build());

        var estoque = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComInsumo("Insumo Saga Falha")
            .ComQuantidadeDisponivel(2)  // Só tem 2
            .ComPreco(100.00m)
            .Build());

        // Act - Criar OS solicitando mais do que o disponível
        var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .ComInsumo(estoque.Id, "Insumo Saga Falha", 10, 100.00m)  // Pede 10, só tem 2
            .Build());

        os.Should().NotBeNull();

        // Assert - Saga deve falhar e compensar
        var sagaFalhou = await AsyncHelper.WaitForConditionAsync(
            async () =>
            {
                var o = await OrdensApi.ObterPorIdAsync(os!.Id);
                return o?.SagaStatus == SagaStatus.Falhou || o?.SagaStatus == SagaStatus.Compensada;
            },
            timeout: TimeSpan.FromSeconds(30));

        sagaFalhou.Should().BeTrue("Saga deveria falhar quando estoque é insuficiente");
    }
}

using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;

namespace CrossService.IntegrationTests.Scenarios.Validacoes;

[Collection("IntegrationTests")]
public class TC04_VeiculoInvalidoTests : IntegrationTestBase
{
    public TC04_VeiculoInvalidoTests(TestContainersFixture containers)
        : base(containers) { }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task CriarOS_ComVeiculoInexistente_DeveRetornarErro()
    {
        // Arrange
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao().Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço Teste")
            .ComValor(100.00m)
            .Disponivel()
            .Build());

        var veiculoIdInexistente = Guid.NewGuid();

        // Act
        var resultado = await OrdensApi.TentarCriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculoIdInexistente)
            .ComServico(servico.Id)
            .ComDescricao("Teste com veículo inválido")
            .Build());

        // Assert
        resultado.Should().BeNull("A criação deveria falhar com veículo inexistente");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task CriarOS_ComVeiculoDeOutroCliente_DeveRetornarErro()
    {
        // Arrange
        var cliente1 = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao().ComNome("Cliente 1").Build());

        var cliente2 = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao().ComNome("Cliente 2").Build());

        var veiculoDoCliente2 = await CadastrosApi.AdicionarVeiculoAsync(cliente2.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente2.Id)
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço Teste")
            .ComValor(100.00m)
            .Disponivel()
            .Build());

        // Act - Tentar criar OS para cliente1 usando veículo do cliente2
        var resultado = await OrdensApi.TentarCriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente1.Id)
            .ParaVeiculo(veiculoDoCliente2.Id)
            .ComServico(servico.Id)
            .ComDescricao("Teste com veículo de outro cliente")
            .Build());

        // Assert
        resultado.Should().BeNull("A criação deveria falhar com veículo de outro cliente");
    }
}

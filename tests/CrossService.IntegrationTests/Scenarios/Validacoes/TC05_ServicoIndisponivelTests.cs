using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;

namespace CrossService.IntegrationTests.Scenarios.Validacoes;

[Collection("IntegrationTests")]
public class TC05_ServicoIndisponivelTests : IntegrationTestBase
{
    public TC05_ServicoIndisponivelTests(TestContainersFixture containers)
        : base(containers) { }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task CriarOS_ComServicoIndisponivel_DeveRetornarErro()
    {
        // Arrange
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao().Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        var servicoIndisponivel = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço Indisponível")
            .ComValor(200.00m)
            .Indisponivel()
            .Build());

        // Act
        var resultado = await OrdensApi.TentarCriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servicoIndisponivel.Id)
            .ComDescricao("Teste com serviço indisponível")
            .Build());

        // Assert
        resultado.Should().BeNull("A criação deveria falhar com serviço indisponível");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task CriarOS_ComServicoInexistente_DeveRetornarErro()
    {
        // Arrange
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao().Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        var servicoIdInexistente = Guid.NewGuid();

        // Act
        var resultado = await OrdensApi.TentarCriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servicoIdInexistente)
            .ComDescricao("Teste com serviço inexistente")
            .Build());

        // Assert
        resultado.Should().BeNull("A criação deveria falhar com serviço inexistente");
    }
}

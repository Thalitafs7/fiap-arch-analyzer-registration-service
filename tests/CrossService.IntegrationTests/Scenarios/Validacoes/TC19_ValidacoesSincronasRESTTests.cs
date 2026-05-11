using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;

namespace CrossService.IntegrationTests.Scenarios.Validacoes;

[Collection("IntegrationTests")]
public class TC19_ValidacoesSincronasRESTTests : IntegrationTestBase
{
    public TC19_ValidacoesSincronasRESTTests(TestContainersFixture containers)
        : base(containers) { }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task ValidarClienteVeiculo_ComDadosValidos_DeveRetornarSucesso()
    {
        // Arrange
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente Validação")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        // Act
        var resultado = await CadastrosApi.ValidarClienteVeiculoAsync(cliente.Id, veiculo.Id);

        // Assert
        resultado.Should().BeTrue("Cliente e veículo válidos devem retornar sucesso");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task ValidarClienteVeiculo_ComClienteInexistente_DeveRetornarFalso()
    {
        // Arrange
        var clienteInexistente = Guid.NewGuid();
        var veiculoInexistente = Guid.NewGuid();

        // Act
        var resultado = await CadastrosApi.ValidarClienteVeiculoAsync(clienteInexistente, veiculoInexistente);

        // Assert
        resultado.Should().BeFalse("Cliente inexistente deve retornar falso");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task ObterServico_ComServicoExistente_DeveRetornarDados()
    {
        // Arrange
        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço REST Test")
            .ComValor(350.00m)
            .Disponivel()
            .Build());

        // Act
        var resultado = await CadastrosApi.ObterServicoAsync(servico.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nome.Should().StartWith("Serviço REST Test");
        resultado.Valor.Should().Be(350.00m);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Medium")]
    public async Task ObterCliente_ComClienteExistente_DeveRetornarDados()
    {
        // Arrange
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente REST Test")
            .Build());

        // Act
        var resultado = await CadastrosApi.ObterClienteAsync(cliente.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be("Cliente REST Test");
    }
}

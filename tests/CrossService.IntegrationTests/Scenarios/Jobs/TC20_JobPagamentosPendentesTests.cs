using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Scenarios.Jobs;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
[Trait("Priority", "Medium")]
public class TC20_JobPagamentosPendentesTests : IntegrationTestBase
{
    public TC20_JobPagamentosPendentesTests(TestContainersFixture containers)
        : base(containers)
    {
    }

    [Fact]
    public async Task ConsultarOSPorStatusPaga_DeveRetornarOSPagas()
    {
        // Arrange
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao().Build());
        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(
            cliente.Id,
            VeiculoBuilder.Padrao().ParaCliente(cliente.Id).Build());
        var servico = await CadastrosApi.CriarServicoAsync(ServicoBuilder.Padrao().Build());

        var ordemServico = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .SemInsumos()
            .Build());

        await OrdensApi.GerarOrcamentoAsync(ordemServico.Id, new GerarOrcamentoRequest(
            ordemServico.Id, servico.Valor, 0.00m, 7));
        await OrdensApi.AprovarOrcamentoAsync(ordemServico.Id, true);
        await OrdensApi.IniciarPagamentoPIXAsync(ordemServico.Id, "test@test.com");

        // Act - Consultar OS por status Paga
        var osPagas = await OrdensApi.ObterPorStatusAsync(StatusOrdemServico.Paga);

        // Assert
        osPagas.Should().NotBeEmpty();
        osPagas.Should().Contain(os => os.Id == ordemServico.Id);
    }
}

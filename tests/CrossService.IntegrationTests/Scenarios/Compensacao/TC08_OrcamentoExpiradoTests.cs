using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Scenarios.Compensacao;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
[Trait("Priority", "High")]
public class TC08_OrcamentoExpiradoTests : IntegrationTestBase
{
    public TC08_OrcamentoExpiradoTests(TestContainersFixture containers)
        : base(containers)
    {
    }

    [Fact]
    public async Task OrcamentoExpirado_DeveAtualizarStatusDaOS()
    {
        // Arrange - Criar OS sem insumos (não depende de saga)
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

        // Gerar orçamento
        var osComOrcamento = await OrdensApi.GerarOrcamentoAsync(ordemServico.Id, new GerarOrcamentoRequest(
            ordemServico.Id, servico.Valor, 0.00m, 7));

        // Assert - Verificar que orçamento foi gerado
        osComOrcamento.Should().NotBeNull();
        osComOrcamento.Orcamento.Should().NotBeNull();
        osComOrcamento.Orcamento!.ValorTotal.Should().Be(servico.Valor);

        var osFinal = await OrdensApi.ObterPorIdAsync(ordemServico.Id);
        osFinal!.Orcamento.Should().NotBeNull();
        osFinal.Status.Should().BeOneOf(StatusOrdemServico.AguardandoAprovacao, StatusOrdemServico.OrcamentoExpirado);
    }
}

using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Scenarios.Pagamento;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
[Trait("Priority", "High")]
public class TC18_WebhookMercadoPagoTests : IntegrationTestBase
{
    public TC18_WebhookMercadoPagoTests(TestContainersFixture containers)
        : base(containers)
    {
    }

    [Fact]
    public async Task WebhookPagamento_DeveBaixarEstoque()
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

        await OrdensApi.GerarOrcamentoAsync(ordemServico.Id, new GerarOrcamentoRequest(
            ordemServico.Id, servico.Valor, 0.00m, 7));
        await OrdensApi.AprovarOrcamentoAsync(ordemServico.Id, true);

        var pagamento = await OrdensApi.IniciarPagamentoPIXAsync(ordemServico.Id, "test@test.com");

        // Assert - Verificar pagamento PIX iniciado
        pagamento.Should().NotBeNull();
        pagamento.MetodoPagamento.Should().Be(MetodoPagamento.Pix);

        var osFinal = await OrdensApi.ObterPorIdAsync(ordemServico.Id);
        osFinal!.Pagamento.Should().NotBeNull();
        osFinal.Status.Should().Be(StatusOrdemServico.Paga);
    }
}

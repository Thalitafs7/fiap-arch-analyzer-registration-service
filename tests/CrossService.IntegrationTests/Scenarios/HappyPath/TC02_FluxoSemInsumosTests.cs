using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Scenarios.HappyPath;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
[Trait("Priority", "Critical")]
public class TC02_FluxoSemInsumosTests : IntegrationTestBase
{
    public TC02_FluxoSemInsumosTests(TestContainersFixture containers)
        : base(containers)
    {
    }

    [Fact]
    public async Task FluxoSemInsumos_DeveCompletarSemReserva()
    {
        // ═══════════════════════════════════════════════════════════════
        // ARRANGE
        // ═══════════════════════════════════════════════════════════════

        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao().Build());
        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(
            cliente.Id,
            VeiculoBuilder.Padrao().ParaCliente(cliente.Id).Build());
        var servico = await CadastrosApi.CriarServicoAsync(ServicoBuilder.Padrao()
            .ComValor(80.00m)
            .Build());

        // ═══════════════════════════════════════════════════════════════
        // ACT - Criar OS sem insumos
        // ═══════════════════════════════════════════════════════════════

        var ordemServico = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .ComDescricao("Diagnóstico computadorizado")
            .SemInsumos()
            .Build());

        // ═══════════════════════════════════════════════════════════════
        // ASSERT 1 - Verificar OS criada
        // ═══════════════════════════════════════════════════════════════

        ordemServico.Should().NotBeNull();
        ordemServico.Insumos.Should().BeEmpty();

        // ═══════════════════════════════════════════════════════════════
        // ACT - Gerar orçamento
        // ═══════════════════════════════════════════════════════════════

        var osComOrcamento = await OrdensApi.GerarOrcamentoAsync(ordemServico.Id, new GerarOrcamentoRequest(
            ordemServico.Id,
            servico.Valor,
            0.00m,
            7));

        osComOrcamento.Should().NotBeNull();
        osComOrcamento.Orcamento.Should().NotBeNull();
        osComOrcamento.Orcamento!.ValorTotal.Should().Be(servico.Valor);

        // ═══════════════════════════════════════════════════════════════
        // ACT - Aprovar orçamento
        // ═══════════════════════════════════════════════════════════════

        var osAposAprovacao = await OrdensApi.AprovarOrcamentoAsync(ordemServico.Id, true);
        osAposAprovacao.Status.Should().Be(StatusOrdemServico.Aprovada);

        // ═══════════════════════════════════════════════════════════════
        // ACT - Pagamento PIX
        // ═══════════════════════════════════════════════════════════════

        var pagamento = await OrdensApi.IniciarPagamentoPIXAsync(ordemServico.Id, "cliente@email.com");

        pagamento.Should().NotBeNull();
        pagamento.MetodoPagamento.Should().Be(MetodoPagamento.Pix);

        // ═══════════════════════════════════════════════════════════════
        // ASSERT FINAL
        // ═══════════════════════════════════════════════════════════════

        var osFinal = await OrdensApi.ObterPorIdAsync(ordemServico.Id);
        osFinal!.Status.Should().Be(StatusOrdemServico.Paga);
        osFinal.Pagamento.Should().NotBeNull();
    }
}

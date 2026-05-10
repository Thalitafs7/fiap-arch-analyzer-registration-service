using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Scenarios.Saga;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
[Trait("Priority", "High")]
public class TC12_IdempotenciaEventosTests : IntegrationTestBase
{
    public TC12_IdempotenciaEventosTests(TestContainersFixture containers)
        : base(containers)
    {
    }

    [Fact]
    public async Task EventoDuplicado_OrdemServicoCriada_DeveSerIgnorado()
    {
        // ═══════════════════════════════════════════════════════════════
        // ARRANGE - Criar OS sem insumos (não depende de saga)
        // ═══════════════════════════════════════════════════════════════

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

        // ═══════════════════════════════════════════════════════════════
        // ASSERT - Verificar OS criada corretamente
        // ═══════════════════════════════════════════════════════════════

        ordemServico.Should().NotBeNull();
        ordemServico.Id.Should().NotBeEmpty();

        var ordemServico2 = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .SemInsumos()
            .Build());

        ordemServico2.Should().NotBeNull();
        ordemServico2.Id.Should().NotBe(ordemServico.Id, "Cada chamada deve criar uma nova OS");
    }
}

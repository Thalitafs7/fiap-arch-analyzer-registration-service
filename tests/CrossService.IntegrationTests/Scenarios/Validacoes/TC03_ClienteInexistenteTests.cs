using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Scenarios.Validacoes;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
[Trait("Priority", "High")]
public class TC03_ClienteInexistenteTests : IntegrationTestBase
{
    public TC03_ClienteInexistenteTests(TestContainersFixture containers)
        : base(containers)
    {
    }

    [Fact]
    public async Task CriarOS_ComClienteInexistente_DeveRetornarErro()
    {
        // Arrange
        var clienteInexistenteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();

        // Act & Assert
        var act = async () => await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(clienteInexistenteId)
            .ParaVeiculo(veiculoId)
            .ComServico(servicoId)
            .SemInsumos()
            .Build());

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}

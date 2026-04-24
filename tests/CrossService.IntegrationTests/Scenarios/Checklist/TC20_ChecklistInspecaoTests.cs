using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Scenarios.Checklist;

[Collection("IntegrationTests")]
public class TC20_ChecklistInspecaoTests : IntegrationTestBase
{
    public TC20_ChecklistInspecaoTests(TestContainersFixture containers)
        : base(containers) { }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task CriarChecklist_ComCategoriasEItens_DeveCalcularResultadoCorretamente()
    {
        // Arrange
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente Checklist")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .ComPlaca($"CHK{new Random().Next(1000, 9999)}")
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Revisão Completa")
            .ComValor(350.00m)
            .Disponivel()
            .Build());

        var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .Build());

        // Act - Criar checklist com itens variados
        var checklist = await OrdensApi.CriarChecklistAsync(new ChecklistBuilder()
            .ParaOrdemServico(os!.Id)
            .ParaVeiculo(veiculo.Id)
            .ComVeiculoSnapshot(veiculo.Placa, "Fiat", "Uno", 2020, "Prata")
            .TipoInspecao("completa")
            .ComInspetor(Guid.NewGuid(), "João Inspetor")
            .ComQuilometragem(85000)
            .ComCategoria("Motor", 1, new[]
            {
                ("MOT-001", "Nível de óleo", StatusItemInspecao.Ok),
                ("MOT-002", "Correia dentada", StatusItemInspecao.Atencao),
                ("MOT-003", "Filtro de ar", StatusItemInspecao.Ok)
            })
            .ComCategoria("Freios", 2, new[]
            {
                ("FRE-001", "Pastilha dianteira", StatusItemInspecao.Critico),
                ("FRE-002", "Disco de freio", StatusItemInspecao.Atencao)
            })
            .Build());

        // Assert
        checklist.Should().NotBeNull();
        checklist!.Resultado.TotalItens.Should().Be(5);
        checklist.Resultado.ItensOk.Should().Be(2);
        checklist.Resultado.ItensAtencao.Should().Be(2);
        checklist.Resultado.ItensCriticos.Should().Be(1);
        checklist.Resultado.StatusGeral.Should().Be(StatusInspecao.Reprovado);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task BuscarChecklistPorVeiculo_DeveRetornarHistoricoOrdenado()
    {
        // Arrange
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente Histórico")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .ComPlaca($"HIS{new Random().Next(1000, 9999)}")
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Manutenção")
            .ComValor(200.00m)
            .Disponivel()
            .Build());

        // Criar 3 checklists em sequência
        for (int i = 0; i < 3; i++)
        {
            var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
                .ParaCliente(cliente.Id)
                .ParaVeiculo(veiculo.Id)
                .ComServico(servico.Id)
                .Build());

            await OrdensApi.CriarChecklistAsync(new ChecklistBuilder()
                .ParaOrdemServico(os!.Id)
                .ParaVeiculo(veiculo.Id)
                .ComVeiculoSnapshot(veiculo.Placa, "Fiat", "Palio", 2019, "Azul")
                .TipoInspecao(i % 2 == 0 ? "entrada" : "saida")
                .ComInspetor(Guid.NewGuid(), "Inspetor")
                .ComQuilometragem(50000 + i * 5000)
                .ComCategoria("Geral", 1, new[]
                {
                    ($"GER-{i:D3}", $"Item {i}", StatusItemInspecao.Ok)
                })
                .Build());

            await Task.Delay(100); // Garantir timestamps diferentes
        }

        // Act
        var historico = await OrdensApi.ObterChecklistsPorVeiculoAsync(veiculo.Id, limite: 10);

        // Assert
        historico.Should().NotBeNull();
        historico!.Count().Should().Be(3);

        var lista = historico.ToList();
        for (int i = 0; i < lista.Count - 1; i++)
        {
            lista[i].DataInspecao.Should().BeOnOrAfter(lista[i + 1].DataInspecao,
                "Histórico deve estar ordenado por data decrescente");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Medium")]
    public async Task ObterProblemasFrequentes_DeveAgregarPorMarcaModelo()
    {
        // Arrange - Criar múltiplos checklists com problemas recorrentes
        for (int i = 0; i < 5; i++)
        {
            var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
                .ComNome($"Cliente Análise {i}")
                .Build());

            var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
                .ParaCliente(cliente.Id)
                .ComPlaca($"ANA{new Random().Next(1000, 9999)}")
                .Build());

            var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
                .ComNome($"Serviço {i}")
                .ComValor(150.00m)
                .Disponivel()
                .Build());

            var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
                .ParaCliente(cliente.Id)
                .ParaVeiculo(veiculo.Id)
                .ComServico(servico.Id)
                .Build());

            // Simular problemas recorrentes
            await OrdensApi.CriarChecklistAsync(new ChecklistBuilder()
                .ParaOrdemServico(os!.Id)
                .ParaVeiculo(veiculo.Id)
                .ComVeiculoSnapshot($"ANA{i:D4}", "Volkswagen", "Gol", 2018 + i, "Branco")
                .TipoInspecao("completa")
                .ComInspetor(Guid.NewGuid(), "Analista")
                .ComQuilometragem(60000 + i * 10000)
                .ComCategoria("Motor", 1, new[]
                {
                    ("MOT-001", "Correia dentada", i % 2 == 0 ? StatusItemInspecao.Atencao : StatusItemInspecao.Ok),
                    ("MOT-002", "Velas", i % 3 == 0 ? StatusItemInspecao.Critico : StatusItemInspecao.Ok)
                })
                .Build());
        }

        // Act
        var problemas = await OrdensApi.ObterProblemasFrequentesAsync("Volkswagen", "Gol", limite: 10);

        // Assert
        problemas.Should().NotBeNull();
        problemas!.Should().NotBeEmpty();

        var lista = problemas.ToList();
        // Verificar ordenação por frequência
        for (int i = 0; i < lista.Count - 1; i++)
        {
            lista[i].Total.Should().BeGreaterThanOrEqualTo(lista[i + 1].Total);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Medium")]
    public async Task ObterEstatisticas_DeveContabilizarPorStatus()
    {
        // Arrange - Criar checklists com diferentes resultados
        var statusSequencia = new[]
        {
            StatusItemInspecao.Ok,      // Aprovado
            StatusItemInspecao.Ok,      // Aprovado
            StatusItemInspecao.Atencao, // Aprovado com ressalvas
            StatusItemInspecao.Critico, // Reprovado
            StatusItemInspecao.Critico  // Reprovado
        };

        foreach (var status in statusSequencia)
        {
            var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao().Build());
            var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
                .ParaCliente(cliente.Id)
                .Build());
            var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
                .ComNome("Serviço Estatística")
                .Disponivel()
                .Build());

            var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
                .ParaCliente(cliente.Id)
                .ParaVeiculo(veiculo.Id)
                .ComServico(servico.Id)
                .Build());

            await OrdensApi.CriarChecklistAsync(new ChecklistBuilder()
                .ParaOrdemServico(os!.Id)
                .ParaVeiculo(veiculo.Id)
                .ComVeiculoSnapshot("EST0000", "Honda", "Civic", 2021, "Preto")
                .TipoInspecao("completa")
                .ComInspetor(Guid.NewGuid(), "Estatístico")
                .ComQuilometragem(30000)
                .ComCategoria("Geral", 1, new[]
                {
                    ("GER-001", "Item único", status)
                })
                .Build());
        }

        // Act
        var estatisticas = await OrdensApi.ObterEstatisticasChecklistAsync(
            inicio: DateTime.UtcNow.AddDays(-1),
            fim: DateTime.UtcNow.AddDays(1));

        // Assert
        estatisticas.Should().NotBeNull();
        estatisticas!.TotalInspecoes.Should().BeGreaterThanOrEqualTo(5);
        estatisticas.Aprovados.Should().BeGreaterThanOrEqualTo(2);
        estatisticas.AprovadosComRessalvas.Should().BeGreaterThanOrEqualTo(1);
        estatisticas.Reprovados.Should().BeGreaterThanOrEqualTo(2);
    }
}

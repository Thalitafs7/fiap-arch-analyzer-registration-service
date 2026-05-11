using CrossService.IntegrationTests.Builders;
using CrossService.IntegrationTests.Common;
using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Scenarios.Saga;

[Collection("IntegrationTests")]
public class TC13_OutboxPatternTests : IntegrationTestBase
{
    public TC13_OutboxPatternTests(TestContainersFixture containers)
        : base(containers) { }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Critical")]
    public async Task CriarOrdemComInsumos_DeveSalvarMensagemNoOutbox()
    {
        // Arrange
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente Outbox")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço Outbox")
            .ComValor(200.00m)
            .Disponivel()
            .Build());

        var estoque = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComInsumo("Insumo Outbox Test")
            .ComQuantidadeDisponivel(100)
            .ComPreco(50.00m)
            .Build());

        // Act - Criar OS COM insumos (deve gerar mensagem no Outbox)
        var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .ComInsumo(estoque.Id, "Insumo Outbox Test", 2, 50.00m)
            .Build());

        // Assert
        os.Should().NotBeNull();
        os!.Id.Should().NotBe(Guid.Empty);

        // Verificar que a mensagem foi salva no Outbox
        var outboxMessage = await OrdensApi.ObterMensagemOutboxPorOrdemServicoAsync(os.Id);
        outboxMessage.Should().NotBeNull("Deve existir mensagem no Outbox para OS com insumos");
        outboxMessage!.EventType.Should().Contain("OrdemServicoCriadaEvent");
        outboxMessage.ProcessedAt.Should().BeNull("Mensagem ainda não foi processada");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Critical")]
    public async Task CriarOrdemSemInsumos_NaoDeveCriarMensagemNoOutbox()
    {
        // Arrange
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente Sem Insumo")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço Simples")
            .ComValor(150.00m)
            .Disponivel()
            .Build());

        // Act - Criar OS SEM insumos (não deve gerar mensagem no Outbox)
        var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .Build()); // Sem insumos

        // Assert
        os.Should().NotBeNull();
        os!.SagaStatus.Should().Be(SagaStatus.Completa, "Saga sem insumos deve completar imediatamente");

        // Verificar que NÃO existe mensagem no Outbox
        var outboxMessage = await OrdensApi.ObterMensagemOutboxPorOrdemServicoAsync(os.Id);
        outboxMessage.Should().BeNull("Não deve existir mensagem no Outbox para OS sem insumos");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task OutboxProcessor_DevePublicarMensagensPendentes()
    {
        // Arrange
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente Processamento")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço Processamento")
            .ComValor(300.00m)
            .Disponivel()
            .Build());

        var estoque = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComInsumo("Insumo Processamento")
            .ComQuantidadeDisponivel(50)
            .ComPreco(75.00m)
            .Build());

        var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .ComInsumo(estoque.Id, "Insumo Processamento", 1, 75.00m)
            .Build());

        // Act - Aguardar processamento do Outbox (background service)
        var mensagemProcessada = await AsyncHelper.WaitForConditionAsync(
            async () =>
            {
                var msg = await OrdensApi.ObterMensagemOutboxPorOrdemServicoAsync(os!.Id);
                return msg?.ProcessedAt != null;
            },
            timeout: TimeSpan.FromSeconds(30));

        // Assert
        mensagemProcessada.Should().BeTrue("OutboxProcessor deveria processar a mensagem");

        var mensagem = await OrdensApi.ObterMensagemOutboxPorOrdemServicoAsync(os!.Id);
        mensagem.Should().NotBeNull();
        mensagem!.ProcessedAt.Should().NotBeNull();
        mensagem.RetryCount.Should().Be(0, "Mensagem processada com sucesso não deve ter retries");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task MensagemOutbox_DeveConterCorrelationId()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();

        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente CorrelationId")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço CorrelationId")
            .ComValor(250.00m)
            .Disponivel()
            .Build());

        var estoque = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComInsumo("Insumo CorrelationId")
            .ComQuantidadeDisponivel(30)
            .ComPreco(80.00m)
            .Build());

        // Act - Criar OS com CorrelationId no header
        var os = await OrdensApi.CriarComCorrelationIdAsync(
            new OrdemServicoBuilder()
                .ParaCliente(cliente.Id)
                .ParaVeiculo(veiculo.Id)
                .ComServico(servico.Id)
                .ComInsumo(estoque.Id, "Insumo CorrelationId", 1, 80.00m)
                .Build(),
            correlationId);

        // Assert
        os.Should().NotBeNull();

        var outboxMessage = await OrdensApi.ObterMensagemOutboxPorOrdemServicoAsync(os!.Id);
        outboxMessage.Should().NotBeNull();
        outboxMessage!.CorrelationId.Should().Be(correlationId,
            "CorrelationId da requisição deve ser propagado para o Outbox");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Critical")]
    public async Task AtomicidadeOutbox_SagaCompleta_DeveReservarEstoque()
    {
        // Arrange
        var cliente = await CadastrosApi.CriarClienteAsync(ClienteBuilder.Padrao()
            .ComNome("Cliente Atomicidade")
            .Build());

        var veiculo = await CadastrosApi.AdicionarVeiculoAsync(cliente.Id, VeiculoBuilder.Padrao()
            .ParaCliente(cliente.Id)
            .Build());

        var servico = await CadastrosApi.CriarServicoAsync(new ServicoBuilder()
            .ComNome("Serviço Atomicidade")
            .ComValor(180.00m)
            .Disponivel()
            .Build());

        var estoque = await EstoqueApi.CriarEstoqueAsync(new EstoqueBuilder()
            .ComInsumo("Insumo Atomicidade")
            .ComQuantidadeDisponivel(20)
            .ComPreco(45.00m)
            .Build());

        var quantidadeInicial = estoque.QuantidadeDisponivel;

        // Act
        var os = await OrdensApi.CriarAsync(new OrdemServicoBuilder()
            .ParaCliente(cliente.Id)
            .ParaVeiculo(veiculo.Id)
            .ComServico(servico.Id)
            .ComInsumo(estoque.Id, "Insumo Atomicidade", 3, 45.00m)
            .Build());

        // Aguardar saga completar
        var sagaCompleta = await AsyncHelper.WaitForConditionAsync(
            async () =>
            {
                var ordem = await OrdensApi.ObterPorIdAsync(os!.Id);
                return ordem?.SagaStatus == SagaStatus.Completa;
            },
            timeout: TimeSpan.FromSeconds(30));

        // Assert
        sagaCompleta.Should().BeTrue("Saga deveria completar com sucesso");

        // Verificar que estoque foi reservado
        var estoqueAtualizado = await EstoqueApi.ObterPorIdAsync(estoque.Id);
        estoqueAtualizado.Should().NotBeNull();
        estoqueAtualizado!.QuantidadeReservada.Should().Be(3,
            "Quantidade solicitada deveria estar reservada");
    }
}
